using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sven.Configurations;
using Sven.Data.Repositories;
using Sven.Models;
using Sven.Services;
using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Sven.Tests.ClientCredentials
{
    [Trait("Category", "ClientCredentials")]
    public class Rfc6749ClientCredentialsGrantTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly SvenWebAppFactory _factory;
        private readonly HttpClient _client;

        // Pre-hashed secret for TestConfidentialClientSecret ("test-confidential-secret-32bytes!")
        private static readonly string HashedSecret = PasswordHasher.HashPassword(TestDataConstants.TestConfidentialClientSecret);
        private const string TestScope = "api.read api.write";

        public Rfc6749ClientCredentialsGrantTests(SvenWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        // Seeds a confidential client into the in-memory store for a single test.
        private async Task SeedConfidentialClientAsync(string scope = TestScope)
        {
            Client confidentialClient = new Client
            {
                Identifier = TestDataConstants.TestConfidentialClientId,
                Name = "Test Confidential Client",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Active = true,
                HashedSecret = HashedSecret,
                Type = AuthConstants.ClientTypes.Confidential,
                RedirectUris = new HashSet<string> { "https://localhost:3000/callback" },
                Scope = new ScopeParameter(scope),
            };

            using IServiceScope serviceScope = _factory.Services.CreateScope();
            IClientRepository store = serviceScope.ServiceProvider.GetRequiredService<IClientRepository>();
            await store.StoreAsync(confidentialClient, CancellationToken.None);
        }

        private static string BuildBasicAuthHeader(string clientId, string secret)
        {
            string credentials = $"{clientId}:{secret}";
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            return $"Basic {encoded}";
        }

        [Fact]
        public async Task Token_ClientSecretBasic_ValidCredentials_Returns200WithJwt()
        {
            // POST /connect/token, grant_type=client_credentials, Authorization: Basic header, valid client.
            await SeedConfidentialClientAsync();

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.ClientCredentials,
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestConfidentialClientId,
            };
            _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(
                BuildBasicAuthHeader(TestDataConstants.TestConfidentialClientId, TestDataConstants.TestConfidentialClientSecret));

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("access_token", content);
        }

        [Fact]
        public async Task Token_ClientSecretPost_ValidCredentials_Returns200WithJwt()
        {
            // POST /connect/token, grant_type=client_credentials, client_id+client_secret in form body.
            await SeedConfidentialClientAsync();

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.ClientCredentials,
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestConfidentialClientId,
                [AuthConstants.OAuth.FieldNames.ClientSecret] = TestDataConstants.TestConfidentialClientSecret,
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("access_token", content);
        }

        [Fact]
        public async Task Token_IssuedToken_HasNoSubClaim()
        {
            // Decode the returned JWT and assert absence of sub claim.
            await SeedConfidentialClientAsync();

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.ClientCredentials,
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestConfidentialClientId,
                [AuthConstants.OAuth.FieldNames.ClientSecret] = TestDataConstants.TestConfidentialClientSecret,
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using JsonDocument doc = JsonDocument.Parse(content);
            string accessToken = doc.RootElement.GetProperty("access_token").GetString()!;

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = handler.ReadJwtToken(accessToken);

            Assert.DoesNotContain(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub);
        }

        [Fact]
        public async Task Token_IssuedToken_ContainsOnlyRegisteredScopes()
        {
            // Request subset scope; returned token scope claim matches requested subset.
            await SeedConfidentialClientAsync(scope: "api.read api.write");

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.ClientCredentials,
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestConfidentialClientId,
                [AuthConstants.OAuth.FieldNames.ClientSecret] = TestDataConstants.TestConfidentialClientSecret,
                [AuthConstants.OAuth.FieldNames.Scope] = "api.read",
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using JsonDocument doc = JsonDocument.Parse(content);
            string accessToken = doc.RootElement.GetProperty("access_token").GetString()!;

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = handler.ReadJwtToken(accessToken);

            System.Security.Claims.Claim? scopeClaim = jwt.Claims.FirstOrDefault(c => c.Type == "scope");
            Assert.NotNull(scopeClaim);
            Assert.Equal("api.read", scopeClaim.Value);
        }

        [Fact]
        public async Task Token_OmittedScope_IssuesAllRegisteredScopes()
        {
            // Omit scope parameter; returned token contains full registered scope.
            await SeedConfidentialClientAsync(scope: "api.read api.write");

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.ClientCredentials,
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestConfidentialClientId,
                [AuthConstants.OAuth.FieldNames.ClientSecret] = TestDataConstants.TestConfidentialClientSecret,
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using JsonDocument doc = JsonDocument.Parse(content);
            string accessToken = doc.RootElement.GetProperty("access_token").GetString()!;

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = handler.ReadJwtToken(accessToken);

            System.Security.Claims.Claim? scopeClaim = jwt.Claims.FirstOrDefault(c => c.Type == "scope");
            Assert.NotNull(scopeClaim);
            // Both scopes should be present (space-separated, order may vary)
            Assert.Contains("api.read", scopeClaim.Value);
            Assert.Contains("api.write", scopeClaim.Value);
        }

        [Fact]
        public async Task Token_ScopeExceedsRegistered_Returns400InvalidScope()
        {
            // Request scope not registered for client; expect 400 invalid_scope.
            await SeedConfidentialClientAsync(scope: "api.read");

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.ClientCredentials,
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestConfidentialClientId,
                [AuthConstants.OAuth.FieldNames.ClientSecret] = TestDataConstants.TestConfidentialClientSecret,
                [AuthConstants.OAuth.FieldNames.Scope] = "api.read api.write",  // api.write not registered
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidScope, content);
        }

        [Fact]
        public async Task Token_UnknownClient_Returns401InvalidClient()
        {
            // Unknown client_id; expect 401 invalid_client.
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.ClientCredentials,
                [AuthConstants.OAuth.FieldNames.ClientId] = "unknown-client-id",
                [AuthConstants.OAuth.FieldNames.ClientSecret] = "any-secret",
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidClient, content);
        }

        [Fact]
        public async Task Token_WrongSecret_Returns401InvalidClient()
        {
            // Correct client_id, wrong secret; expect 401 invalid_client.
            await SeedConfidentialClientAsync();

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.ClientCredentials,
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestConfidentialClientId,
                [AuthConstants.OAuth.FieldNames.ClientSecret] = "wrong-secret",
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidClient, content);
        }

        [Fact]
        public async Task Token_BothAuthMethods_Returns400InvalidRequest()
        {
            // Both Basic header and form client_secret present; expect 400 invalid_request.
            await SeedConfidentialClientAsync();

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.ClientCredentials,
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestConfidentialClientId,
                [AuthConstants.OAuth.FieldNames.ClientSecret] = TestDataConstants.TestConfidentialClientSecret,  // Post method
            };
            _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(
                BuildBasicAuthHeader(TestDataConstants.TestConfidentialClientId, TestDataConstants.TestConfidentialClientSecret));  // Basic method

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidRequest, content);
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
