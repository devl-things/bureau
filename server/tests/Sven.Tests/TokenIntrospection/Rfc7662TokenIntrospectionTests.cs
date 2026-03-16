using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sven.Configurations;
using Sven.Data.Repositories;
using Sven.Models;
using Sven.Services;
using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using System.Text;
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

        [Fact]
        public async Task ValidToken_Returns200_WithActiveTrue_AndClaims()
        {
            // authenticated client, valid JWT → 200 active:true, sub, scope, exp, iat, jti, iss present
            Assert.Fail("not implemented");
            // TODO: await SeedConfidentialClientAsync(); issue token via /connect/token;
            //       POST /oidc/introspect with Authorization: Basic header + token param;
            //       assert 200, active=true, sub, scope, exp, iat, jti, iss in response JSON.
        }

        [Fact]
        public async Task ExpiredToken_Returns200_WithActiveFalse_NoExtraClaims()
        {
            // authenticated client, expired JWT → 200 {"active":false} only
            Assert.Fail("not implemented");
            // TODO: construct an expired JWT; POST /oidc/introspect; assert 200, active=false,
            //       no sub/scope/exp/iat/jti/iss fields.
        }

        [Fact]
        public async Task MalformedToken_Returns200_WithActiveFalse()
        {
            // authenticated client, non-JWT string → 200 {"active":false}
            Assert.Fail("not implemented");
            // TODO: POST /oidc/introspect with token="not-a-jwt"; assert 200, active=false.
        }

        [Fact]
        public async Task UnknownToken_Returns200_WithActiveFalse()
        {
            // authenticated client, well-formed JWT with wrong signature → 200 {"active":false}
            Assert.Fail("not implemented");
            // TODO: craft a JWT signed with a different key; POST /oidc/introspect;
            //       assert 200, active=false.
        }

        [Fact]
        public async Task UnauthenticatedCaller_Returns401_InvalidClient()
        {
            // no Authorization header, valid token → 401 invalid_client
            Assert.Fail("not implemented");
            // TODO: POST /oidc/introspect with no Authorization header and a valid token param;
            //       assert 401, error=invalid_client.
        }

        [Fact]
        public async Task MissingTokenField_Returns400_InvalidRequest()
        {
            // authenticated client, empty form body → 400 invalid_request
            Assert.Fail("not implemented");
            // TODO: POST /oidc/introspect with Authorization header but empty form body;
            //       assert 400, error=invalid_request.
        }

        [Fact]
        public async Task MachineToken_Returns200_WithActiveTrueAndNoSub()
        {
            // authenticated client, machine token (from CreateMachineTokenAsync) → 200 active:true, no sub field
            Assert.Fail("not implemented");
            // TODO: issue a machine token via client_credentials grant; POST /oidc/introspect;
            //       assert 200, active=true, no sub claim in response JSON.
        }

        [Fact]
        public async Task IntrospectionEndpoint_PresentInDiscoveryDocument()
        {
            // GET /.well-known/openid-configuration → introspection_endpoint present in JSON response
            Assert.Fail("not implemented");
            // TODO: GET /.well-known/openid-configuration; parse JSON;
            //       assert introspection_endpoint key is present and points to /oidc/introspect.
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
