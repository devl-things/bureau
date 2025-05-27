using Bureau.Core;
using Sven.Data;
using Sven.Models;

namespace Sven.Services
{
    public class UserProvider : IUserProvider
    {
        // TODO move to configuration
        private const int MAX_VERIFICATION_CODE = 999999;
        private const int MIN_VERIFICATION_CODE = 100000;

        private readonly IStore<string, string> _miscStore;
        private readonly IUserStore _userStore;
        public UserProvider(IStore<string, string> miscStore, IUserStore userStore)
        {
            _miscStore = miscStore;
            _userStore = userStore;
        }

        public int MinVerificationCode { get { return MIN_VERIFICATION_CODE; } }

        public int MaxVerificationCode { get { return MAX_VERIFICATION_CODE; } }

        public async Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            SvenUser user = new SvenUser()
            {
                DisplayName = email,
                Username = email,
                PasswordHash = PasswordHasher.HashPassword(password),
            };
            if (await _userStore.StoreAsync(user, cancellationToken) is { IsError: true } storedResult)
            {
                return storedResult.Error;
            }
            if (await _miscStore.RemoveAsync(email, cancellationToken) is { IsError: true } codeRemovalResult)
            {
                return codeRemovalResult.Error;
            }
            return new Result<string>(user.SubjectId);
        }

        public Task<bool> ExistsUserWithEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _userStore.ExistsWithEmail(email, cancellationToken);
        }

        public async Task<Result<string>> GenerateVerificationCodeAsync(string email, CancellationToken cancellationToken)
        {
            string code = new Random().Next(MinVerificationCode, MaxVerificationCode).ToString();

            // TODO add expiration date 60min
            if (await _miscStore.StoreAsync(email, code, cancellationToken) is { IsError: true } result)
            {
                return result.Error;
            }
            return new Result<string>(code);
        }

        public async Task<Result<string>> GenerateTicketAsync(string userIdentifier, CancellationToken cancellationToken = default)
        {
            string ticket = new Random().Next(MinVerificationCode, MaxVerificationCode).ToString();
            // TODO add expiration date 5min
            if (await _miscStore.StoreAsync(ticket, userIdentifier, cancellationToken) is { IsError: true } result)
            {
                return result.Error;
            }
            return new Result<string>(ticket);
        }

        public Task<Result<string>> GetVerificationCodeAsync(string email, CancellationToken cancellationToken)
        {
            return _miscStore.GetAsync(email, cancellationToken);
        }

        public Task<Result<string>> GetUserIdByTicketAsync(string ticket, CancellationToken cancellationToken)
        {
            return _miscStore.GetAsync(ticket, cancellationToken);
        }
    }
}
