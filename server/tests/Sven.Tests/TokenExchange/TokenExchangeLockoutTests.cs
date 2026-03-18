using Xunit;

namespace Sven.Tests.TokenExchange
{
    public class TokenExchangeLockoutTests
    {
        /// <summary>
        /// After 5 consecutive failures for (clientId, userId), the 6th exchange attempt
        /// returns an error without executing validation steps 1-4.
        /// </summary>
        [Fact]
        public void AfterNConsecutiveFailures_TokenExchangeIsRefused()
        {
            Assert.Fail("SEC-05 not implemented: lockout after N failures");
        }

        /// <summary>
        /// A successful token exchange clears the failure count for (clientId, userId),
        /// so subsequent failures restart the counter from 0.
        /// </summary>
        [Fact]
        public void SuccessfulExchange_ResetsFailureCounter()
        {
            Assert.Fail("SEC-05 not implemented: success resets counter");
        }

        /// <summary>
        /// After the lockout cooldown window (15 min) expires, the next exchange attempt
        /// proceeds normally (counter cleared automatically by IsLockedOutAsync).
        /// </summary>
        [Fact]
        public void AfterCooldownExpires_ExchangeIsAllowedAgain()
        {
            Assert.Fail("SEC-05 not implemented: cooldown expiry re-enables exchange");
        }
    }
}
