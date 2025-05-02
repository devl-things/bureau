using Sven.Services;
using Sven.Tests.TestData;
using Xunit.Abstractions;

namespace Sven.Tests.Services
{
    public class PasswordHasherTests
    {
        private readonly ITestOutputHelper _output;
        public PasswordHasherTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        [Trait("Category", "Unit")]
        public void HashPassword_ReturnsPasswordHash()
        {
            string passwordHash = PasswordHasher.HashPassword(TestDataConstants.TestUserPassword);

            _output.WriteLine($"For password '{TestDataConstants.TestUserPassword}' hash is {passwordHash}");

            Assert.NotNull(passwordHash);
            PasswordHasher.VerifyPassword(passwordHash, TestDataConstants.TestUserPassword);
        }
    }
}
