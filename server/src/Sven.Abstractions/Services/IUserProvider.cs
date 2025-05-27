
using Bureau.Core;

namespace Sven.Services
{
    public interface IUserProvider
    {
        public int MinVerificationCode { get; }
        public int MaxVerificationCode { get; }
        Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<bool> ExistsUserWithEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<string>> GenerateVerificationCodeAsync(string email, CancellationToken cancellationToken);
        Task<Result<string>> GenerateTicketAsync(string userIdentifier, CancellationToken cancellationToken = default);
        Task<Result<string>> GetVerificationCodeAsync(string email, CancellationToken cancellationToken);
        Task<Result<string>> GetUserIdByTicketAsync(string ticket, CancellationToken cancellationToken);
    }
}
