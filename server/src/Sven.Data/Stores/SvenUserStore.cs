using Bureau.Core;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Models;

namespace Sven.Data.Stores
{
    internal class SvenUserStore : IUserStore
    {
        private readonly SvenContext _context;
        private readonly TimeProvider _timeProvider;
        public SvenUserStore(SvenContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }
        public async Task<Result<SvenUser>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new ResultError($"{nameof(username)} field not defined.");
            }
            UserDb? user = await _context.Users.Where(x => x.Username.Equals(username)).FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return new ResultError($"User does not exist.");
            }
            SvenUser result = new SvenUser();
            result.DisplayName = user.DisplayName;
            result.PasswordHash = user.PasswordHash;
            result.SubjectId = user.Identifier;
            result.Username = user.Username;

            return result;
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
    }
}
