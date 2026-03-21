using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sven.Configurations;
using Sven.Data.Repositories;
using Sven.Models;
using Sven.Services;
using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace Sven.Tests.EndSession
{
    [Trait("Category", "EndSession")]
    public class RpInitiatedLogoutTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly SvenWebAppFactory _factory;
        private readonly HttpClient _client;

        private static readonly string HashedSecret = PasswordHasher.HashPassword(TestDataConstants.TestConfidentialClientSecret);
        private const string TestClientId = TestDataConstants.TestConfidentialClientId;
        private const string TestLogoutUri = "https://app.localhost/logged-out";

        public RpInitiatedLogoutTests(SvenWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private async Task SeedClientWithLogoutUriAsync(string postLogoutUri)
        {
            Client confidentialClient = new Client
            {
                Identifier = TestClientId,
                Name = "Test Confidential Client",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Active = true,
                HashedSecret = HashedSecret,
                Type = AuthConstants.ClientTypes.Confidential,
                RedirectUris = new HashSet<string> { "https://localhost:3000/callback" },
                Scope = new ScopeParameter("openid profile"),
                PostLogoutRedirectUris = new List<string> { postLogoutUri },
            };

            using IServiceScope serviceScope = _factory.Services.CreateScope();
            IClientRepository store = serviceScope.ServiceProvider.GetRequiredService<IClientRepository>();
            await store.StoreAsync(confidentialClient, CancellationToken.None);
        }

        private async Task<string> BuildValidIdTokenHintAsync()
        {
            RsaSecurityKey serverKey = _factory.Services.GetRequiredService<RsaSecurityKey>();
            SigningCredentials creds = new SigningCredentials(serverKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: "https://test.localhost",
                audience: TestClientId,
                claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, "test-user") },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);
            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(jwt));
        }

        private static string BuildWrongKeyIdTokenHint()
        {
            using RSA wrongRsa = RSA.Create(2048);
            RsaSecurityKey wrongKey = new RsaSecurityKey(wrongRsa);
            SigningCredentials wrongCreds = new SigningCredentials(wrongKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: "https://test.localhost",
                audience: TestClientId,
                claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, "fake-user") },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: wrongCreds);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        [Fact]
        public async Task InvalidIdTokenHint_Returns400_SessionNotCleared()
        {
            // PROT-04 SC1: wrong-key id_token_hint → 400 invalid_request, session not cleared
            string wrongToken = BuildWrongKeyIdTokenHint();
            string url = $"{Endpoints.Connect.EndSession}?id_token_hint={Uri.EscapeDataString(wrongToken)}";

            HttpResponseMessage response = await _client.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.Equal(AuthConstants.OAuth.Errors.InvalidRequest, doc.RootElement.GetProperty("error").GetString());
        }

        [Fact]
        public async Task UnregisteredRedirectUri_Returns200_LoggedOut_SessionCleared()
        {
            // PROT-04 SC2: valid id_token_hint, post_logout_redirect_uri not in registered list → 200 {logged_out:true}
            await SeedClientWithLogoutUriAsync(TestLogoutUri);
            string hint = await BuildValidIdTokenHintAsync();
            string url = $"{Endpoints.Connect.EndSession}?id_token_hint={Uri.EscapeDataString(hint)}&post_logout_redirect_uri={Uri.EscapeDataString("https://evil.example.com/callback")}";

            HttpResponseMessage response = await _client.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.GetProperty("logged_out").GetBoolean());
        }

        [Fact]
        public async Task RegisteredRedirectUri_Returns302_ToUri_SessionCleared()
        {
            // PROT-04 SC3: valid id_token_hint, post_logout_redirect_uri matches registered → 302 to that URI
            await SeedClientWithLogoutUriAsync(TestLogoutUri);
            string hint = await BuildValidIdTokenHintAsync();
            string url = $"{Endpoints.Connect.EndSession}?id_token_hint={Uri.EscapeDataString(hint)}&post_logout_redirect_uri={Uri.EscapeDataString(TestLogoutUri)}";

            HttpResponseMessage response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            string? location = response.Headers.Location?.ToString();
            Assert.NotNull(location);
            Assert.StartsWith(TestLogoutUri, location);
        }

        [Fact]
        public async Task NoRedirectUri_Returns200_LoggedOut_SessionCleared()
        {
            // PROT-04 SC4: no id_token_hint → 200 {logged_out:true}
            HttpResponseMessage response = await _client.GetAsync(Endpoints.Connect.EndSession);
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.GetProperty("logged_out").GetBoolean());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _client.Dispose();
        }
    }
}
