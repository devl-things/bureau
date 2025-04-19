using Microsoft.AspNetCore.Mvc.Testing;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;
using Sven.Tests.Fixtures;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Web;

namespace Sven.Tests.Controllers
{
    public class ConnectControllerTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly HttpClient _client;

        public ConnectControllerTests(SvenWebAppFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true,
                AllowAutoRedirect = true // We want to inspect the redirect response manually
            });
        }

        [Fact(DisplayName = "oauth with pkce gets access, refresh and id token")]
        [Trait("Category", "Integration")]
        [Trait("Type", "Happy path")]
        public async Task FullOAuthFlow_ReturnsAllTokens()
        {
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = AuthCodeProvider.GenerateCodeChallenge(codeVerifier);
            string redirectUriExpected = "https://localhost:3000/callback";
            string clientId = "test-client";
            string nonceExpected = "some_random_nonce";

            // Step 1: /connect/authorize
            string authorizeUrl = $"/{Endpoints.Connect.Base}/{Endpoints.Connect.AuthorizePath}?response_type={AuthConstants.OAuth.ResponseTypes.Code}&client_id={clientId}&redirect_uri={redirectUriExpected}" +
                $"&scope={AuthConstants.Scopes.OfflineAccess} {AuthConstants.Scopes.OpenId}&code_challenge={codeChallenge}&code_challenge_method={AuthConstants.OAuth.CodeChallengeMethods.Sha256}&state=test-state" +
                $"&{AuthConstants.OAuth.FieldNames.Nonce}={nonceExpected}";

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
            string? code = HttpUtility.ParseQueryString(new Uri(redirectUriActual).Query).Get(AuthConstants.OAuth.ResponseTypes.Code);
            Assert.False(string.IsNullOrWhiteSpace(code));

            // Step 3: Exchange code for access + refresh token
            HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantTypes.AuthorizationCode },
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
            Assert.False(string.IsNullOrWhiteSpace(token1.IdToken));
            // validate nonce in authorization code flow
            string? nonceTokenActual = GetClaimFromIdToken(token1.IdToken!, JwtRegisteredClaimNames.Nonce);
            Assert.Equal(nonceExpected, nonceTokenActual);

            // Step 4: Use refresh token once (should succeed)
            HttpRequestMessage refreshRequest1 = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            refreshRequest1.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantTypes.RefreshToken },
                { AuthConstants.OAuth.FieldNames.RefreshToken, token1.RefreshToken },
                { AuthConstants.OAuth.FieldNames.ClientId, clientId },
                { AuthConstants.OAuth.FieldNames.Scope, AuthConstants.Scopes.OpenId}
            });

            HttpResponseMessage refreshResponse1 = await _client.SendAsync(refreshRequest1);

            Assert.Equal(HttpStatusCode.OK, refreshResponse1.StatusCode);
            tokenJson = await refreshResponse1.Content.ReadAsStringAsync();
            SvenToken token2 = JsonSerializer.Deserialize<SvenToken>(tokenJson)!;
            Assert.NotEqual(token1.AccessToken, token2.AccessToken);
            Assert.False(string.IsNullOrWhiteSpace(token2.IdToken));
            // validate nonce in refresh token flow
            nonceTokenActual = GetClaimFromIdToken(token2.IdToken!, JwtRegisteredClaimNames.Nonce);
            Assert.Equal(nonceExpected, nonceTokenActual);
            // validate that new refresh token is rotated
            Assert.False(string.IsNullOrWhiteSpace(token2.RefreshToken));
            Assert.NotEqual(token1.RefreshToken, token2.RefreshToken);

            // Step 5: Reuse same refresh token again (should fail)
            HttpRequestMessage refreshRequest2 = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            refreshRequest2.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantTypes.RefreshToken },
                { AuthConstants.OAuth.FieldNames.RefreshToken, token1.RefreshToken }, // reusing the same token
                { AuthConstants.OAuth.FieldNames.ClientId, clientId }
            });

            HttpResponseMessage refreshResponse2 = await _client.SendAsync(refreshRequest2);
            Assert.Equal(HttpStatusCode.BadRequest, refreshResponse2.StatusCode);
        }

        [Fact(DisplayName = "bad request if openid scope is no defined the first time but is on refresh")]
        [Trait("Category", "Integration")]
        [Trait("Type", "Expected error")]
        public async Task FullOAuthFlow_WithoutOpenIdScope_DoesNotIssueIdTokenEvenInRefresh()
        {
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = AuthCodeProvider.GenerateCodeChallenge(codeVerifier);
            string redirectUriExpected = "https://localhost:3000/callback";
            string clientId = "test-client";
            string nonceExpected = "some_random_nonce";

            // Step 1: /connect/authorize
            string authorizeUrl = $"/{Endpoints.Connect.Base}/{Endpoints.Connect.AuthorizePath}?response_type={AuthConstants.OAuth.ResponseTypes.Code}&client_id={clientId}&redirect_uri={redirectUriExpected}" +
                $"&scope={AuthConstants.Scopes.OfflineAccess}&code_challenge={codeChallenge}&code_challenge_method={AuthConstants.OAuth.CodeChallengeMethods.Sha256}&state=test-state" +
                $"&{AuthConstants.OAuth.FieldNames.Nonce}={nonceExpected}";

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
            string? code = HttpUtility.ParseQueryString(new Uri(redirectUriActual).Query).Get(AuthConstants.OAuth.ResponseTypes.Code);
            Assert.False(string.IsNullOrWhiteSpace(code));

            // Step 3: Exchange code for access + refresh token
            HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantTypes.AuthorizationCode },
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
            Assert.True(string.IsNullOrWhiteSpace(token1.IdToken));

            // Step 4: Use refresh token once (should succeed)
            HttpRequestMessage refreshRequest1 = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            refreshRequest1.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantTypes.RefreshToken },
                { AuthConstants.OAuth.FieldNames.RefreshToken, token1.RefreshToken },
                { AuthConstants.OAuth.FieldNames.ClientId, clientId },
                { AuthConstants.OAuth.FieldNames.Scope, AuthConstants.Scopes.OpenId}
            });

            HttpResponseMessage refreshResponse1 = await _client.SendAsync(refreshRequest1);

            Assert.Equal(HttpStatusCode.BadRequest, refreshResponse1.StatusCode);
        }

        [Fact(DisplayName = "id token is not provided if on refresh openid scope is not repeated")]
        [Trait("Category", "Integration")]
        [Trait("Type", "Expected behaviour")]
        public async Task Refresh_WithoutOpenIdScope_DoesNotIssueIdToken()
        {
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = AuthCodeProvider.GenerateCodeChallenge(codeVerifier);
            string redirectUriExpected = "https://localhost:3000/callback";
            string clientId = "test-client";
            string nonceExpected = "some_random_nonce";

            // Step 1: /connect/authorize
            string authorizeUrl = $"/{Endpoints.Connect.Base}/{Endpoints.Connect.AuthorizePath}?response_type={AuthConstants.OAuth.ResponseTypes.Code}&client_id={clientId}&redirect_uri={redirectUriExpected}" +
                $"&scope={AuthConstants.Scopes.OfflineAccess} {AuthConstants.Scopes.OpenId}&code_challenge={codeChallenge}&code_challenge_method={AuthConstants.OAuth.CodeChallengeMethods.Sha256}&state=test-state" +
                $"&{AuthConstants.OAuth.FieldNames.Nonce}={nonceExpected}";

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
            string? code = HttpUtility.ParseQueryString(new Uri(redirectUriActual).Query).Get(AuthConstants.OAuth.ResponseTypes.Code);
            Assert.False(string.IsNullOrWhiteSpace(code));

            // Step 3: Exchange code for access + refresh token
            HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantTypes.AuthorizationCode },
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
            Assert.False(string.IsNullOrWhiteSpace(token1.IdToken));
            // validate nonce in authorization code flow
            string? nonceTokenActual = GetClaimFromIdToken(token1.IdToken!, JwtRegisteredClaimNames.Nonce);
            Assert.Equal(nonceExpected, nonceTokenActual);

            // Step 4: Use refresh token once (should succeed)
            HttpRequestMessage refreshRequest1 = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            refreshRequest1.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantTypes.RefreshToken },
                { AuthConstants.OAuth.FieldNames.RefreshToken, token1.RefreshToken },
                { AuthConstants.OAuth.FieldNames.ClientId, clientId }
            });

            HttpResponseMessage refreshResponse1 = await _client.SendAsync(refreshRequest1);

            Assert.Equal(HttpStatusCode.OK, refreshResponse1.StatusCode);
            tokenJson = await refreshResponse1.Content.ReadAsStringAsync();
            SvenToken token2 = JsonSerializer.Deserialize<SvenToken>(tokenJson)!;
            Assert.NotEqual(token1.AccessToken, token2.AccessToken);
            Assert.True(string.IsNullOrWhiteSpace(token2.IdToken));
        }

        [Fact(DisplayName = "oauth with pkce gets access but not refresh and id token")]
        [Trait("Category", "Integration")]
        public async Task FullOAuthFlow_WithoutOfflineAccess_DoesNotIssueRefreshToken()
        {
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = AuthCodeProvider.GenerateCodeChallenge(codeVerifier);
            string redirectUriExpected = "https://localhost:3000/callback";
            string clientId = "test-client";

            // Step 1: /connect/authorize
            string authorizeUrl = $"/{Endpoints.Connect.Base}/{Endpoints.Connect.AuthorizePath}?response_type={AuthConstants.OAuth.ResponseTypes.Code}&client_id={clientId}&redirect_uri={redirectUriExpected}" +
                $"&scope=email&code_challenge={codeChallenge}&code_challenge_method={AuthConstants.OAuth.CodeChallengeMethods.Sha256}&state=test-state";

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
            string? code = HttpUtility.ParseQueryString(new Uri(redirectUriActual).Query).Get(AuthConstants.OAuth.ResponseTypes.Code);
            Assert.False(string.IsNullOrWhiteSpace(code));

            // Step 3: Exchange code for access + refresh token
            HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.GrantTypeField, AuthConstants.OAuth.GrantTypes.AuthorizationCode },
                { AuthConstants.OAuth.FieldNames.Code, code! },
                { AuthConstants.OAuth.FieldNames.CodeVerifier, codeVerifier },
                { AuthConstants.OAuth.FieldNames.ClientId, clientId }
            });

            HttpResponseMessage tokenResponse = await _client.SendAsync(tokenRequest);

            Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
            string tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            SvenToken token = JsonSerializer.Deserialize<SvenToken>(tokenJson)!;

            Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
            Assert.True(string.IsNullOrWhiteSpace(token.RefreshToken));
            Assert.True(string.IsNullOrWhiteSpace(token.IdToken));
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

        private static string? GetClaimFromIdToken(string idToken, string claimType)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.ReadJwtToken(idToken);
            return token.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
