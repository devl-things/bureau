using Bureau.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;


using Sven.Configurations;
using Sven.Models;
using Sven.Tests.TestData;

namespace Sven.Data.Tests.Stores
{
    public class ClientStoreTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        public ClientStoreTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Type", "Happy path")]
        public async Task StoreAsync_PersistsClient()
        {
            Client client = new Client
            {
                Identifier = TestDataConstants.TestClientId,
                Name = "Test client",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Active = true,
                RedirectUris = new HashSet<string> { "https://localhost:3000/callback" },
                Scope = new ScopeParameter($"{AuthConstants.Scopes.OpenId} {AuthConstants.Scopes.Email} {AuthConstants.Scopes.OfflineAccess}"),
                Type = AuthConstants.ClientTypes.Public,
                AccessTokenLifetime = new TimeSpan(0, 59, 50),
                IdTokenLifetime = new TimeSpan(0, 4, 50),
                RefreshTokenLifetime = new TimeSpan(29, 23, 59, 50),
            };

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                IClientStore store = scope.ServiceProvider.GetRequiredService<IClientStore>();

                Result result = await store.StoreAsync(client, CancellationToken.None);
                Assert.True(result.IsSuccess);

                Result<Client> savedClientResult = await store.GetAsync(client.Identifier, CancellationToken.None);
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
