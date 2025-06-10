using Bureau.Core;
using Sven.Configurations;
using Sven.Data;
using Sven.Models;

namespace Sven.Services
{
    public class UserProvider : IUserProvider
    {
        // #53 move to configuration
        private const int MAX_VERIFICATION_CODE = 999999;
        private const int MIN_VERIFICATION_CODE = 100000;

        private readonly IStore<string, string> _miscStore;
        private readonly IStore<string, UserVerificationCode> _verificationCodeStore;
        private readonly IUserStore _userStore;
        private readonly TimeProvider _timeProvider;
        public UserProvider(IStore<string, string> miscStore, IStore<string, UserVerificationCode> verificationCodeStore, IUserStore userStore, TimeProvider timeProvider)
        {
            _miscStore = miscStore;
            _verificationCodeStore = verificationCodeStore;
            _userStore = userStore;
            _timeProvider = timeProvider;
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

        public async Task<Result> UpdatePasswordAsync(string userId, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return new ResultError(ErrorMessages.PasswordEmpty);
            }
            Result<SvenUser> userResult = await _userStore.GetByIdentifierAsync(userId, cancellationToken);
            if (userResult.IsError)
            {
                return userResult.Error;
            }
            userResult.Value.PasswordHash = PasswordHasher.HashPassword(password);
            if (await _userStore.StoreAsync(userResult.Value, cancellationToken) is { IsError: true } storedResult)
            {
                return storedResult.Error;
            }
            return true;
        }

        private Task<Result<string>> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _userStore.GetByUsernameAsync(email, cancellationToken)
                .ContinueWith(x =>
                {
                    if (x.Result.IsError)
                    {
                        return x.Result.Error;
                    }
                    return new Result<string>(x.Result.Value.SubjectId);
                }, cancellationToken);
        }
        public Task<bool> ExistsUserWithEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _userStore.ExistsWithEmail(email, cancellationToken);
        }

        public async Task<Result<string>> GenerateTicketAsync(string userIdentifier, CancellationToken cancellationToken = default)
        {
            // #53 Make sure that using this pseudorandom number generator is safe here csharpsquid:S2245
            string ticket = new Random().Next(MinVerificationCode, MaxVerificationCode).ToString();
            // #53 add expiration date 5min
            if (await _miscStore.StoreAsync(ticket, userIdentifier, cancellationToken) is { IsError: true } result)
            {
                return result.Error;
            }
            return new Result<string>(ticket);
        }

        public Task<Result<UserVerificationCode>> GenerateVerificationCodeAsync(string email, CancellationToken cancellationToken = default)
        {
            // #53 every generation of verification code needs to invalid all other verification codes for the same email 
            return GenerateVerificationCodeAsync(email, VerificationStatus.None, cancellationToken);
        }
        public async Task<Result<UserVerificationCode>> GenerateVerificationCodeAsync(string email, VerificationStatus status, CancellationToken cancellationToken = default)
        {
            Result<string> userIdResult = await GetUserIdByEmailAsync(email, cancellationToken);
            // #53 Make sure that using this pseudorandom number generator is safe here csharpsquid:S2245         
            string code = new Random().Next(MinVerificationCode, MaxVerificationCode).ToString();
            UserVerificationCode data = new(email, code, userIdResult.Value, status, _timeProvider.GetUtcNow().AddMinutes(60));
            if (await _verificationCodeStore.StoreAsync(data.Id, data, cancellationToken) is { IsError: true } result)
            {
                return result.Error;
            }
            return new Result<UserVerificationCode>(data);
        }
        public async Task<Result<UserVerificationCode>> RegenerateVerificationCodeAsync(UserVerificationCode verificationCode, CancellationToken cancellationToken = default)
        {
            string code = new Random().Next(MinVerificationCode, MaxVerificationCode).ToString();
            UserVerificationCode data = new(verificationCode.Email, code, verificationCode.UserId, VerificationStatus.None, _timeProvider.GetUtcNow().AddMinutes(60));
            if (await _verificationCodeStore.StoreAsync(data.Id, data, cancellationToken) is { IsError: true } result)
            {
                return result.Error;
            }
            return new Result<UserVerificationCode>(data);
        }

        public async Task<Result<UserVerificationCode>> GetVerificationCodeAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out _))
            {
                return new ResultError($"Verification code Id = {id} malformed");
            }
            Result<UserVerificationCode> result = await _verificationCodeStore.GetAsync(id, cancellationToken);
            if (result.Value.Expiration < _timeProvider.GetUtcNow())
            {
                return new ResultError($"Verification code Id = {id} expired");
            }
            return result;
        }

        public async Task<Result> UpdateVerificationCodeStatusAsync(string codeId, VerificationStatus status, CancellationToken cancellationToken = default)
        {
            Result<UserVerificationCode> result = await GetVerificationCodeAsync(codeId, cancellationToken);
            if (result.IsError)
            {
                return result.Error;
            }
            result.Value.Status = status;
            if (VerificationStatus.Invalid.Equals(status))
            {
                result.Value.Expiration = _timeProvider.GetUtcNow(); // invalidate the code
            }
            return true;
        }

        public Task<Result<string>> GetUserIdByTicketAsync(string ticket, CancellationToken cancellationToken = default)
        {
            return _miscStore.GetAsync(ticket, cancellationToken);
        }

        public async Task<Result> DeleteUserAsync(string email, CancellationToken cancellationToken = default)
        {
            Result<SvenUser> userResult = await _userStore.GetByUsernameAsync(email, cancellationToken);
            if (userResult.IsError)
            {
                return userResult.Error;
            }
            if (await _userStore.RemoveAsync(userResult.Value.SubjectId, cancellationToken) is { IsError: true } storedResult)
            {
                return storedResult.Error;
            }
            return true;
        }
    }
}
