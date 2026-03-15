using Bureau;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sven.Data.Repositories;
using Sven.Models;
using Sven.Services;
using Sven.Tests.TestData;

namespace Sven.Data.Tests.Stores
{
    public class SvenUserStoreTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        public SvenUserStoreTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Type", "Happy path")]
        public async Task StoreAsync_PersistsUser()
        {
            SvenUser user = new SvenUser
            {
                SubjectId = "test-user",
                Username = TestDataConstants.TestUserUsername,
                DisplayName = "Test User",
                PasswordHash = PasswordHasher.HashPassword(TestDataConstants.TestUserPassword)
            };

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                IUserRepository store = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                Result result = await store.StoreAsync(user, CancellationToken.None);
                Assert.True(result.IsSuccess);

                Result<SvenUser> savedClientResult = await store.GetByUsernameAsync(user.Username, CancellationToken.None);
                Assert.True(savedClientResult.IsSuccess);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _factory.Dispose();
        }
    }
}
