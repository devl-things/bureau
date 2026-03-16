using Microsoft.AspNetCore.Mvc.Testing;
using Sven.Configurations;
using Sven.Tests.Fixtures;
using Xunit;

namespace Sven.Tests.EndSession
{
    [Trait("Category", "EndSession")]
    public class RpInitiatedLogoutTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly SvenWebAppFactory _factory;
        private readonly HttpClient _client;

        public RpInitiatedLogoutTests(SvenWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public void InvalidIdTokenHint_Returns400_SessionNotCleared()
        {
            // PROT-04 SC1: wrong-key or wrong-issuer id_token_hint → 400 invalid_request, session not cleared
            Assert.Fail("not implemented");
        }

        [Fact]
        public void UnregisteredRedirectUri_Returns200_LoggedOut_SessionCleared()
        {
            // PROT-04 SC2: valid id_token_hint, post_logout_redirect_uri not in client's registered list → 200 {logged_out:true}, session cleared
            Assert.Fail("not implemented");
        }

        [Fact]
        public void RegisteredRedirectUri_Returns302_ToUri_SessionCleared()
        {
            // PROT-04 SC3: valid id_token_hint, post_logout_redirect_uri matches registered → 302 to that URI, session cleared
            Assert.Fail("not implemented");
        }

        [Fact]
        public void NoRedirectUri_Returns200_LoggedOut_SessionCleared()
        {
            // PROT-04 SC4: no id_token_hint → 200 {logged_out:true}, session cleared
            Assert.Fail("not implemented");
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
