using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using Xunit;

namespace Sven.Tests.ClientCredentials
{
    [Trait("Category", "ClientCredentials")]
    public class Rfc6749ClientCredentialsGrantTests : IClassFixture<SvenWebAppFactory>
    {
        private readonly SvenWebAppFactory _factory;

        public Rfc6749ClientCredentialsGrantTests(SvenWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Token_ClientSecretBasic_ValidCredentials_Returns200WithJwt()
        {
            // POST /connect/token, grant_type=client_credentials, Authorization: Basic header, valid client.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Token_ClientSecretPost_ValidCredentials_Returns200WithJwt()
        {
            // POST /connect/token, grant_type=client_credentials, client_id+client_secret in form body.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Token_IssuedToken_HasNoSubClaim()
        {
            // Decode the returned JWT and assert absence of sub claim.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Token_IssuedToken_ContainsOnlyRegisteredScopes()
        {
            // Request subset scope; returned token scope claim matches requested subset.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Token_OmittedScope_IssuesAllRegisteredScopes()
        {
            // Omit scope parameter; returned token contains full registered scope.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Token_ScopeExceedsRegistered_Returns400InvalidScope()
        {
            // Request scope not registered for client; expect 400 invalid_scope.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Token_UnknownClient_Returns401InvalidClient()
        {
            // Unknown client_id; expect 401 invalid_client.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Token_WrongSecret_Returns401InvalidClient()
        {
            // Correct client_id, wrong secret; expect 401 invalid_client.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void Token_BothAuthMethods_Returns400InvalidRequest()
        {
            // Both Basic header and form client_secret present; expect 400 invalid_request.
            Assert.Fail("not implemented");
        }
    }
}
