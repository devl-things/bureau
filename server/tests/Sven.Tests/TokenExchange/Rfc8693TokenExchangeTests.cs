using Sven.Tests.Fixtures;
using Xunit;

namespace Sven.Tests.TokenExchange
{
    [Trait("Category", "TokenExchange")]
    public class Rfc8693TokenExchangeTests : IClassFixture<SvenWebAppFactory>
    {
        private readonly SvenWebAppFactory _factory;

        public Rfc8693TokenExchangeTests(SvenWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public void HappyPath_ValidExchange_ReturnsExternalToken()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void UnknownClient_ReturnsInvalidClient()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void FeatureNotAllowed_ReturnsInvalidScope()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void InvalidSubjectToken_ReturnsInvalidGrant()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void ExpiredSubjectToken_ReturnsInvalidGrant()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void FeatureNotInUserScope_ReturnsInvalidGrant()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void NoTokenAvailable_ReturnsInvalidGrant()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void HouseholdClaims_MemberToken_IncludesHouseholdIdAndRole()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void NoHouseholdClaims_SoloUser_ClaimsAbsent()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void SoloUserResponse_HasAccessTokenNoBureauTokens()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void HouseholdAggregation_HasAccessTokenAndBureauTokens()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void HouseholdFallback_NoOwnToken_HasBureauTokensOnly()
        {
            Assert.Fail("Not implemented");
        }
    }
}
