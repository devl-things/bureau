using Bureau;
using Microsoft.AspNetCore.Http;
using Sven.Models;

namespace Sven.Services
{
    internal interface IClientAuthService
    {
        Task<Result<Client>> AuthenticateClientAsync(
            HttpRequest request, CancellationToken cancellationToken = default);
    }
}
