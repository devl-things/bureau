using Bureau;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven;

namespace Sven.Data.Stores
{
    internal class HouseholdService : IHouseholdService
    {
        private readonly SvenContext _context;

        public HouseholdService(SvenContext context)
        {
            _context = context;
        }

        public async Task<Result<HouseholdMembership>> GetMembershipAsync(string userId, CancellationToken cancellationToken = default)
        {
            HouseholdMemberDb? member = await _context.HouseholdMembers
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (member == null)
                return ResultError.From($"User {userId} is not a member of any household.");

            HouseholdDb? household = await _context.Households
                .FirstOrDefaultAsync(x => x.Identifier == member.HouseholdIdentifier, cancellationToken);

            if (household == null)
                return ResultError.From($"Household {member.HouseholdIdentifier} not found.");

            return new HouseholdMembership
            {
                HouseholdId = household.Identifier,
                HouseholdName = household.Name,
                UserId = userId,
                Role = member.Role,
            };
        }

        public async Task<Result<SharedExternalTokenConsent?>> GetActiveSharedTokenAsync(
            string householdId, string provider, string featureKey, CancellationToken cancellationToken = default)
        {
            SharedExternalTokenDb? db = await _context.SharedExternalTokens
                .FirstOrDefaultAsync(x =>
                    x.HouseholdIdentifier == householdId &&
                    x.Provider == provider &&
                    x.FeatureKey == featureKey &&
                    x.RevokedAt == null,
                    cancellationToken);

            if (db == null)
                return new Result<SharedExternalTokenConsent?>(null);

            return new Result<SharedExternalTokenConsent?>(new SharedExternalTokenConsent
            {
                Identifier = db.Identifier,
                HouseholdId = db.HouseholdIdentifier,
                OwnerUserId = db.OwnerUserId,
                Provider = db.Provider,
                FeatureKey = db.FeatureKey,
                SharedAt = db.SharedAt,
            });
        }
    }
}
