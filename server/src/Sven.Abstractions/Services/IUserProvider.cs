
using Bureau.Core;
using Sven.Models;

namespace Sven.Services
{
    public interface IUserProvider
    {
        public int MinVerificationCode { get; }
        public int MaxVerificationCode { get; }
        Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<bool> ExistsUserWithEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<UserVerificationCode>> GenerateVerificationCodeAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<UserVerificationCode>> GenerateVerificationCodeAsync(string email, VerificationStatus status, CancellationToken cancellationToken = default);
        Task<Result<string>> GenerateTicketAsync(string userIdentifier, CancellationToken cancellationToken = default);
        Task<Result<UserVerificationCode>> GetVerificationCodeAsync(string id, CancellationToken cancellationToken = default);
        Task<Result<string>> GetUserIdByTicketAsync(string ticket, CancellationToken cancellationToken = default);
        Task<Result> UpdateVerificationCodeAsync(UserVerificationCode code, CancellationToken cancellationToken = default);
        Task<Result> DeleteUserAsync(string email, CancellationToken cancellationToken = default);
    }
}
