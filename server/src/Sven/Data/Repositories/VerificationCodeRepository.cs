using Bureau;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Models;

namespace Sven.Data.Repositories
{
    internal sealed class VerificationCodeRepository
    {
        private readonly SvenContext _context;
        private readonly TimeProvider _timeProvider;

        public VerificationCodeRepository(SvenContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<Result> StoreAsync(string codeId, UserVerificationCode code, CancellationToken cancellationToken = default)
        {
            UserVerificationCodeDb db = new UserVerificationCodeDb
            {
                Id = codeId,
                Email = code.Email,
                VerificationCode = code.VerificationCode,
                UserId = code.UserId,
                Status = (int)code.Status,
                Expiration = code.Expiration,
                CreatedAt = _timeProvider.GetUtcNow()
            };
            _context.VerificationCodes.Add(db);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Result<UserVerificationCode>> GetAsync(string codeId, CancellationToken cancellationToken = default)
        {
            UserVerificationCodeDb? db = await _context.VerificationCodes
                .FirstOrDefaultAsync(x => x.Id == codeId, cancellationToken);

            if (db == null)
            {
                return ResultError.From($"Verification code Id = {codeId} not found.");
            }

            return MapToDomain(db);
        }

        public async Task<Result> UpdateStatusAsync(string codeId, VerificationStatus status, DateTimeOffset expiration, CancellationToken cancellationToken = default)
        {
            int rows = await _context.VerificationCodes
                .Where(x => x.Id == codeId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.Status, (int)status)
                        .SetProperty(x => x.Expiration, expiration),
                    cancellationToken);

            if (rows == 0)
            {
                return ResultError.From($"Verification code Id = {codeId} not found.");
            }

            return true;
        }

        public async Task<Result> RemoveAsync(string codeId, CancellationToken cancellationToken = default)
        {
            await _context.VerificationCodes
                .Where(x => x.Id == codeId)
                .ExecuteDeleteAsync(cancellationToken);
            return true;
        }

        private static UserVerificationCode MapToDomain(UserVerificationCodeDb db)
        {
            return new UserVerificationCode(
                db.Email,
                db.VerificationCode,
                db.UserId,
                (VerificationStatus)db.Status,
                db.Expiration)
            {
                Id = db.Id
            };
        }
    }
}
