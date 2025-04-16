using Bureau.Core;
using Sven.Abstractions.Services;

namespace Sven.Services
{
    public class InMemoryClientProvider : IClientProvider
    {
        public Task<Result<bool>> IsValidAsync(string clientId, string redirectUri, string scope, CancellationToken cancellationToken = default)
        {
            bool isValid = !(string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri) || string.IsNullOrWhiteSpace(scope));
            return Task.FromResult(new Result<bool>(isValid));
        }
    }
}
