using Bureau;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Mappers;
using Sven.Data.Models;
using Sven;

namespace Sven.Data.Repositories
{
    internal class UserRepository : IUserRepository, Sven.Data.IUserStore
    {
        private readonly SvenContext _context;
        private readonly TimeProvider _timeProvider;
        public UserRepository(SvenContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }
        // #53 is it username or email!!
        public async Task<Result<SvenUser>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return ResultError.From($"{nameof(username)} field not defined.");
            }
            UserDb? user = await _context.Users.Where(x => x.Username.Equals(username)).FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return ResultError.From($"User does not exist.");
            }

            return user.ToSvenUser();
        }

        public async Task<Result<SvenUser>> GetByIdentifierAsync(string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ResultError.From($"{nameof(userId)} field not defined.");
            }
            UserDb? user = await GetUserDbAsync(userId, cancellationToken);

            if (user == null)
            {
                return ResultError.From($"User does not exist.");
            }
            return user.ToSvenUser();
        }

        private Task<UserDb?> GetUserDbAsync(string identifier, CancellationToken cancellationToken = default)
        {
            return _context.Users.FirstOrDefaultAsync(x => x.Identifier.Equals(identifier), cancellationToken);
        }

        public async Task<Result> StoreAsync(SvenUser user, CancellationToken cancellationToken = default)
        {
            UserDb? dbEntity = await GetUserDbAsync(user.SubjectId, cancellationToken);
            if (dbEntity == null)
            {
                dbEntity = new UserDb();
                dbEntity.Identifier = user.SubjectId;
                dbEntity.Username = user.Username;

                dbEntity.CreatedAt = _timeProvider.GetUtcNow();
                dbEntity.CreatedBy = "admin";

                _context.Attach(dbEntity);
            }

            dbEntity.UpdatedAt = _timeProvider.GetUtcNow();
            dbEntity.UpdatedBy = "admin";

            dbEntity.DisplayName = user.DisplayName;
            dbEntity.PasswordHash = user.PasswordHash;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public Task<bool> ExistsWithEmail(string email, CancellationToken cancellationToken)
        {
            // #53 add emails
            return _context.Users.AnyAsync(x => x.Username == email);
        }

        public async Task<Result> RemoveAsync(string userId, CancellationToken cancellationToken)
        {
            UserDb? dbEntity = await GetUserDbAsync(userId, cancellationToken);

            if (dbEntity == null)
            {
                return ResultError.From($"User does not exist.");
            }
            _context.Users.Remove(dbEntity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
