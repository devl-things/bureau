using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using Xunit;

namespace Sven.Tests.ClientCredentials
{
    [Trait("Category", "ClientCredentials")]
    public class Rfc7591ConfidentialClientRegistrationTests : IClassFixture<SvenWebAppFactory>
    {
        private readonly SvenWebAppFactory _factory;

        public Rfc7591ConfidentialClientRegistrationTests(SvenWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Register_WithValidIat_ConfidentialClient_Returns201WithSecret()
        {
            // POST /oidc/register with Authorization: Bearer <iat>, token_endpoint_auth_method=client_secret_basic;
            // expect 201 + client_secret in response.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Register_WithoutIat_Returns401()
        {
            // POST /oidc/register for confidential client without Authorization header; expect 401.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Register_WithWrongIat_Returns401()
        {
            // POST /oidc/register with wrong IAT value; expect 401.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Register_SecretReturnedOnce_NotRecoverableAfterwards()
        {
            // After registration, perform a second GET or re-register and confirm client_secret is absent.
            Assert.Fail("not implemented");
        }
    }
}
