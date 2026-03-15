using Sven.Tests.TestData;
using Xunit;

namespace Sven.Tests.ClientCredentials
{
    [Trait("Category", "ClientCredentials")]
    public class ClientAuthServiceTests
    {
        [Fact]
        public void AuthenticateClientAsync_ClientSecretBasic_ValidCredentials_ReturnsClient()
        {
            // Basic auth header with valid client_id:secret returns Result<Client> without error.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void AuthenticateClientAsync_ClientSecretPost_ValidCredentials_ReturnsClient()
        {
            // Form fields client_id + client_secret with valid credentials returns Result<Client>.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void AuthenticateClientAsync_BothMethodsPresent_Returns400InvalidRequest()
        {
            // Header Basic AND form client_secret present simultaneously returns error invalid_request.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void AuthenticateClientAsync_UnknownClient_Returns401InvalidClient()
        {
            // Non-existent client_id returns error invalid_client.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void AuthenticateClientAsync_WrongSecret_Returns401InvalidClient()
        {
            // Correct client_id, wrong secret returns error invalid_client.
            Assert.Fail("not implemented");
        }

        [Fact]
        public void AuthenticateClientAsync_PublicClient_NoSecret_Rejected()
        {
            // Client with no stored secret (public client) is rejected by IClientAuthService.
            Assert.Fail("not implemented");
        }
    }
}
