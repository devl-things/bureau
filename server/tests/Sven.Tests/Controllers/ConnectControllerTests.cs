using Microsoft.AspNetCore.Mvc.Testing;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;
using Sven.Tests.Fixtures;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
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
        public async Task FullOAuthFlow_HappyPath()
        {
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = AuthCodeProvider.GenerateCodeChallenge(codeVerifier);
            string redirectUriExpected = "https://localhost:3000/callback";
            string clientId = "test-client";

            // Step 1: /connect/authorize
            string authorizeUrl = $"/{Endpoints.Connect.Base}/{Endpoints.Connect.AuthorizePath}?response_type={AuthConstants.OAuth.ResponseType.Code}&client_id={clientId}&redirect_uri={redirectUriExpected}" +
                $"&scope=profile user&code_challenge={codeChallenge}&code_challenge_method={AuthConstants.OAuth.CodeChallengeMethods.Sha256}&state=test-state";

            HttpResponseMessage authorizeResponse = await _client.GetAsync(authorizeUrl);

            Assert.Equal(HttpStatusCode.OK, authorizeResponse.StatusCode);
            string? setCookieHeader = authorizeResponse.RequestMessage?.Headers.GetValues("Cookie").FirstOrDefault();
            Assert.False(string.IsNullOrWhiteSpace(setCookieHeader));
            string pkceKey = ExtractCookieValue(setCookieHeader!, AuthConstants.CookieNames.PkceKey);
            Assert.False(string.IsNullOrWhiteSpace(pkceKey));

            // Step 2: Login with user/pass and pkce_key cookie
            HttpRequestMessage loginRequest = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.AuthorizeLogin);
            loginRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", "admin" },
                { "password", "admin123" }
            });
            loginRequest.Headers.Add("Cookie", $"{AuthConstants.CookieNames.PkceKey}={pkceKey}");

            HttpResponseMessage loginResponse = await _client.SendAsync(loginRequest);

            string? redirectUriActual = loginResponse.RequestMessage?.RequestUri?.ToString();
            Assert.False(string.IsNullOrWhiteSpace(redirectUriActual));
            Assert.StartsWith(redirectUriExpected, redirectUriActual);
            string? code = HttpUtility.ParseQueryString(new Uri(redirectUriActual).Query).Get(AuthConstants.OAuth.ResponseType.Code);
            Assert.False(string.IsNullOrWhiteSpace(code));

            // Step 3: Exchange code for access + refresh token
            HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantType.AuthorizationCode },
                { AuthConstants.OAuth.FieldNames.Code, code! },
                { AuthConstants.OAuth.FieldNames.CodeVerifier, codeVerifier },
                { AuthConstants.OAuth.FieldNames.ClientId, clientId }
            });

            HttpResponseMessage tokenResponse = await _client.SendAsync(tokenRequest);

            Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
            string tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            SvenToken token1 = JsonSerializer.Deserialize<SvenToken>(tokenJson)!;

            Assert.False(string.IsNullOrWhiteSpace(token1.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(token1.RefreshToken));

            // Step 4: Use refresh token once (should succeed)
            HttpRequestMessage refreshRequest1 = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            refreshRequest1.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantType.RefreshToken },
                { AuthConstants.OAuth.FieldNames.RefreshToken, token1.RefreshToken },
                { AuthConstants.OAuth.FieldNames.ClientId, clientId }
            });

            HttpResponseMessage refreshResponse1 = await _client.SendAsync(refreshRequest1);

            Assert.Equal(HttpStatusCode.OK, refreshResponse1.StatusCode);
            tokenJson = await refreshResponse1.Content.ReadAsStringAsync();
            SvenToken token2 = JsonSerializer.Deserialize<SvenToken>(tokenJson)!;
            Assert.NotEqual(token1.AccessToken, token2.AccessToken);

            // Step 5: Reuse same refresh token again (should fail)
            HttpRequestMessage refreshRequest2 = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            refreshRequest2.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantType.RefreshToken },
                { AuthConstants.OAuth.FieldNames.RefreshToken, token1.RefreshToken }, // reusing the same token
                { AuthConstants.OAuth.FieldNames.ClientId, clientId }
            });

            HttpResponseMessage refreshResponse2 = await _client.SendAsync(refreshRequest2);
            Assert.Equal(HttpStatusCode.BadRequest, refreshResponse2.StatusCode);
        }


        [Fact]
        public async Task Token_WithInvalidRefreshToken_ReturnsBadRequest()
        {
            string clientId = "test-client";
            HttpRequestMessage refreshRequest = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            refreshRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantType.RefreshToken },
                { AuthConstants.OAuth.FieldNames.RefreshToken, "invalid_or_expired_token" },
                { AuthConstants.OAuth.FieldNames.ClientId, clientId }
            });

            HttpResponseMessage response = await _client.SendAsync(refreshRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
