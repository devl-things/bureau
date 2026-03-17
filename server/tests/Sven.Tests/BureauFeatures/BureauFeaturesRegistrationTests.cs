using Microsoft.AspNetCore.Mvc.Testing;
using Sven.Tests.Fixtures;
using Xunit;

namespace Sven.Tests.BureauFeatures
{
    [Trait("Category", "BureauFeatures")]
    public class BureauFeaturesRegistrationTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly SvenWebAppFactory _factory;
        private readonly HttpClient _client;

        public BureauFeaturesRegistrationTests(SvenWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public void Register_WithValidBureauFeatures_Returns201WithFeaturesEchoed()
        {
            Assert.Fail("VAULT-01 SC1 stub — implement after Plan 03");
        }

        [Fact]
        public void Register_WithUnknownFeatureKey_Returns400WithDescriptiveError()
        {
            Assert.Fail("VAULT-01 SC2 stub — implement after Plan 03");
        }

        [Fact]
        public void GetClient_ReturnsBureauFeaturesAfterRegistration()
        {
            Assert.Fail("VAULT-01 SC3 stub — implement after Plan 03");
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
