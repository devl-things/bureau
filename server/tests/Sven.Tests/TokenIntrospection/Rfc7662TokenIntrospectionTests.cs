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
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Sven.Tests.TokenIntrospection
{
    [Trait("Category", "TokenIntrospection")]
    public class Rfc7662TokenIntrospectionTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly SvenWebAppFactory _factory;
        private readonly HttpClient _client;

        // Pre-hashed secret for TestConfidentialClientSecret ("test-confidential-secret-32bytes!")
        private static readonly string HashedSecret = PasswordHasher.HashPassword(TestDataConstants.TestConfidentialClientSecret);
        private const string TestScope = "api.read api.write";

        public Rfc7662TokenIntrospectionTests(SvenWebAppFactory factory)
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

        // Obtains a machine token via POST /connect/token with client_credentials grant.
        private async Task<string> ObtainMachineTokenAsync()
        {
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.ClientCredentials,
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestConfidentialClientId,
                [AuthConstants.OAuth.FieldNames.ClientSecret] = TestDataConstants.TestConfidentialClientSecret,
            };

            HttpResponseMessage tokenResponse = await _client.PostAsync(
                Endpoints.Connect.Token,
                new FormUrlEncodedContent(formData));

            string tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(tokenContent);
            return doc.RootElement.GetProperty("access_token").GetString()!;
        }

        [Fact]
        public async Task ValidToken_Returns200_WithActiveTrue_AndClaims()
        {
            // authenticated client, valid JWT → 200 active:true, scope, exp, iat, jti, iss present
            await SeedConfidentialClientAsync();
            string accessToken = await ObtainMachineTokenAsync();

            Dictionary<string, string> introspectForm = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.Token] = accessToken,
            };
            _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(
                BuildBasicAuthHeader(TestDataConstants.TestConfidentialClientId, TestDataConstants.TestConfidentialClientSecret));

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Introspect,
                new FormUrlEncodedContent(introspectForm));
            string content = await response.Content.ReadAsStringAsync();

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.GetProperty("active").GetBoolean());
            Assert.True(doc.RootElement.TryGetProperty("scope", out _));
            Assert.True(doc.RootElement.TryGetProperty("exp", out _));
            Assert.True(doc.RootElement.TryGetProperty("iat", out _));
            Assert.True(doc.RootElement.TryGetProperty("jti", out _));
            Assert.True(doc.RootElement.TryGetProperty("iss", out _));
        }

        [Fact]
        public async Task ExpiredToken_Returns200_WithActiveFalse_NoExtraClaims()
        {
            // authenticated client, expired JWT (server-signed, exp in past) → 200 {"active":false} only
            await SeedConfidentialClientAsync();

            RsaSecurityKey serverKey = _factory.Services.GetRequiredService<RsaSecurityKey>();
            SigningCredentials creds = new SigningCredentials(serverKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken expiredJwt = new JwtSecurityToken(
                issuer: "https://test.localhost",
                claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, "user1") },
                expires: DateTime.UtcNow.AddHours(-1),
                signingCredentials: creds);
            string expiredToken = handler.WriteToken(expiredJwt);

            Dictionary<string, string> introspectForm = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.Token] = expiredToken,
            };
            _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(
                BuildBasicAuthHeader(TestDataConstants.TestConfidentialClientId, TestDataConstants.TestConfidentialClientSecret));

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Introspect,
                new FormUrlEncodedContent(introspectForm));
            string content = await response.Content.ReadAsStringAsync();

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.False(doc.RootElement.GetProperty("active").GetBoolean());
            Assert.False(doc.RootElement.TryGetProperty("sub", out _));
            Assert.False(doc.RootElement.TryGetProperty("scope", out _));
            Assert.False(doc.RootElement.TryGetProperty("exp", out _));
            Assert.False(doc.RootElement.TryGetProperty("iat", out _));
            Assert.False(doc.RootElement.TryGetProperty("jti", out _));
            Assert.False(doc.RootElement.TryGetProperty("iss", out _));
            Assert.False(doc.RootElement.TryGetProperty("client_id", out _));
        }

        [Fact]
        public async Task MalformedToken_Returns200_WithActiveFalse()
        {
            // authenticated client, non-JWT string → 200 {"active":false}
            await SeedConfidentialClientAsync();

            Dictionary<string, string> introspectForm = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.Token] = "not-a-jwt",
            };
            _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(
                BuildBasicAuthHeader(TestDataConstants.TestConfidentialClientId, TestDataConstants.TestConfidentialClientSecret));

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Introspect,
                new FormUrlEncodedContent(introspectForm));
            string content = await response.Content.ReadAsStringAsync();

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.False(doc.RootElement.GetProperty("active").GetBoolean());
        }

        [Fact]
        public async Task UnknownToken_Returns200_WithActiveFalse()
        {
            // authenticated client, well-formed JWT with wrong signature → 200 {"active":false}
            await SeedConfidentialClientAsync();

            // Construct a JWT signed with a freshly generated key (not the server's key)
            using RSA wrongRsa = RSA.Create(2048);
            RsaSecurityKey wrongKey = new RsaSecurityKey(wrongRsa);
            SigningCredentials wrongCreds = new SigningCredentials(wrongKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken wrongJwt = new JwtSecurityToken(
                issuer: "https://test.localhost",
                audience: "test-audience",
                claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, "fake-subject") },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: wrongCreds);
            string wrongSignatureToken = handler.WriteToken(wrongJwt);

            Dictionary<string, string> introspectForm = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.Token] = wrongSignatureToken,
            };
            _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(
                BuildBasicAuthHeader(TestDataConstants.TestConfidentialClientId, TestDataConstants.TestConfidentialClientSecret));

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Introspect,
                new FormUrlEncodedContent(introspectForm));
            string content = await response.Content.ReadAsStringAsync();

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.False(doc.RootElement.GetProperty("active").GetBoolean());
        }

        [Fact]
        public async Task UnauthenticatedCaller_Returns401_InvalidClient()
        {
            // no Authorization header, valid token → 401 invalid_client
            // Seed so there's a valid client, but don't set auth header
            await SeedConfidentialClientAsync();

            Dictionary<string, string> introspectForm = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.Token] = "any-token-value",
            };

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Introspect,
                new FormUrlEncodedContent(introspectForm));
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidClient, content);
        }

        [Fact]
        public async Task MissingTokenField_Returns400_InvalidRequest()
        {
            // authenticated client, empty form body → 400 invalid_request
            await SeedConfidentialClientAsync();

            _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(
                BuildBasicAuthHeader(TestDataConstants.TestConfidentialClientId, TestDataConstants.TestConfidentialClientSecret));

            // Send empty form body — no token field
            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Introspect,
                new FormUrlEncodedContent(new Dictionary<string, string>()));
            string content = await response.Content.ReadAsStringAsync();

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidRequest, content);
        }

        [Fact]
        public async Task MachineToken_Returns200_WithActiveTrueAndNoSub()
        {
            // authenticated client, machine token (from client_credentials) → 200 active:true, no sub field
            await SeedConfidentialClientAsync();
            string accessToken = await ObtainMachineTokenAsync();

            Dictionary<string, string> introspectForm = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.Token] = accessToken,
            };
            _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(
                BuildBasicAuthHeader(TestDataConstants.TestConfidentialClientId, TestDataConstants.TestConfidentialClientSecret));

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Introspect,
                new FormUrlEncodedContent(introspectForm));
            string content = await response.Content.ReadAsStringAsync();

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.GetProperty("active").GetBoolean());
            Assert.False(doc.RootElement.TryGetProperty("sub", out _));
        }

        [Fact]
        public async Task IntrospectionEndpoint_PresentInDiscoveryDocument()
        {
            // GET /.well-known/openid-configuration → introspection_endpoint present in JSON response
            HttpResponseMessage response = await _client.GetAsync(Endpoints.WellKnown.OpenConfiguration);
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("introspection_endpoint", out JsonElement endpoint));
            Assert.EndsWith(Endpoints.Oidc.Introspect, endpoint.GetString()!);
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
