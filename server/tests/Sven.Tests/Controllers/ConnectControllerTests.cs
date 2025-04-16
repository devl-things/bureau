using Microsoft.AspNetCore.Mvc.Testing;
using Sven.Configurations;
using Sven.Services;
using Sven.Tests.Fixtures;
using System.Net;
using System.Security.Cryptography;
using System.Web;

namespace Sven.Tests.Controllers
{
    public class ConnectControllerTests : IClassFixture<SvenWebAppFactory>
    {
        private readonly HttpClient _client;
        private readonly SvenWebAppFactory _factory;

        public ConnectControllerTests(SvenWebAppFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true,
                AllowAutoRedirect = true // We want to inspect the redirect response manually
            });
        }

        [Fact]
        public async Task Authorize_Login_Token_ReturnsAccessToken_WithValidPKCE()
        {
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = AuthCodeProvider.GenerateCodeChallenge(codeVerifier);
            string redirectUriExpected = "https://localhost:3000/callback";
            string clientId = "test-client";


            string authorizeUrl = $"/{Endpoints.Connect.Base}/{Endpoints.Connect.AuthorizePath}?response_type={AuthConstants.OAuth.ResponseType.Code}&client_id={clientId}&redirect_uri={redirectUriExpected}" +
                $"&scope=profile user&code_challenge={codeChallenge}&code_challenge_method={AuthConstants.OAuth.CodeChallengeMethods.Sha256}&state=test-state";

            HttpRequestMessage authorizeRequest = new HttpRequestMessage(HttpMethod.Get, authorizeUrl);
            HttpResponseMessage authorizeResponse = await _client.SendAsync(authorizeRequest);

            Assert.Equal(HttpStatusCode.OK, authorizeResponse.StatusCode);

            string? setCookieHeader = authorizeResponse.RequestMessage?.Headers.GetValues("Cookie").FirstOrDefault();
            Assert.NotNull(setCookieHeader);
            string pkceKey = ExtractCookieValue(setCookieHeader!, AuthConstants.CookieNames.PkceKey);

            // Step 3: Login with user/pass and pkce_key cookie
            HttpRequestMessage loginRequest = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.AuthorizeLogin);
            loginRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", "admin" },
                { "password", "admin123" }
            });
            loginRequest.Headers.Add("Cookie", $"{AuthConstants.CookieNames.PkceKey}={pkceKey}");

            HttpResponseMessage loginResponse = await _client.SendAsync(loginRequest);

            string? redirectUriActual = loginResponse.RequestMessage?.RequestUri?.ToString();
            Assert.NotNull(redirectUriActual);
            Assert.StartsWith(redirectUriExpected, redirectUriActual);
            string? code = HttpUtility.ParseQueryString(new Uri(redirectUriActual).Query).Get(AuthConstants.OAuth.ResponseType.Code);
            Assert.False(string.IsNullOrWhiteSpace(code));

            // Step 4: Exchange code for token
            HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", AuthConstants.OAuth.GrantType.AuthorizationCode },
                { "code", code! },
                { "code_verifier", codeVerifier },
                { "client_id", clientId }
            });

            HttpResponseMessage tokenResponse = await _client.SendAsync(tokenRequest);
            Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);

            string responseBody = await tokenResponse.Content.ReadAsStringAsync();
            Assert.Contains("access_token", responseBody);
            Assert.Contains("token_type", responseBody);
        }


        private static string GenerateCodeVerifier()
        {
            byte[] bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string ExtractCookieValue(string setCookieHeader, string cookieName)
        {
            string prefix = cookieName + "=";
            int start = setCookieHeader.IndexOf(prefix);
            if (start == -1) return string.Empty;

            int end = setCookieHeader.IndexOf(';', start);
            if (end == -1) end = setCookieHeader.Length;

            return setCookieHeader.Substring(start + prefix.Length, end - start - prefix.Length);
        }
    }
}
