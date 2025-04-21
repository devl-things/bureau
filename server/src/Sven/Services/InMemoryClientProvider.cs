using Bureau.Core;
using Sven.Abstractions.Services;

namespace Sven.Services
{
    public class InMemoryClientProvider : IClientProvider
    {
        public Task<Result<bool>> IsScopeValidAsync(string clientId, string scope, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Result<bool>(!string.IsNullOrWhiteSpace(scope)));
        }
        public Task<Result<bool>> IsValidAsync(string clientId, string redirectUri, CancellationToken cancellationToken = default)
        {
            bool isValid = !(string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri));
            return Task.FromResult(new Result<bool>(isValid));
        }
    }
}
