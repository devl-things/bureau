using Bureau;
using Sven;

namespace Sven.Data
{
    /// <summary>
    /// Household store — placeholder. Full invite/sharing flow to be implemented later.
    /// Schema is stable; no structural changes needed for future features.
    /// </summary>
    public interface IHouseholdService
    {
        Task<Result<HouseholdMembership>> GetMembershipAsync(string userId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Returns an active shared token consent record, or null if none exists.
        /// Used by token exchange to fulfil requests via a household member's token.
        /// </summary>
        Task<Result<SharedExternalTokenConsent?>> GetActiveSharedTokenAsync(string householdId, string provider, string featureKey, CancellationToken cancellationToken = default);
        /// <summary>
        /// Returns all active shared token consent records for the household, provider, and feature key.
        /// Empty list means no shared tokens exist for that combination — not an error.
        /// </summary>
        Task<Result<IReadOnlyList<SharedExternalTokenConsent>>> GetAllActiveSharedTokensAsync(string householdId, string provider, string featureKey, CancellationToken cancellationToken = default);
    }
}
