using Bureau;

namespace Niles.Chores.Services
{
    public interface IHouseKeepingService
    {
        Task<PagedResult<Housekeeping>> GetHousekeepingsAsync(SearchParameters searchParameters, CancellationToken cancellationToken = default);
        /// <summary>
        /// Creates a new <see cref="Housekeeping"/> entry and persists it to the database, including any completed chores,
        /// and updates related state (critical chores and caches).
        /// </summary>
        /// <param name="housekeeping">
        /// The domain model to create. The <see cref="Housekeeping.Id"/> will be assigned after persistence.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> that contains the created <see cref="Housekeeping"/> on success.
        /// On failure, returns a <see cref="ResultError"/> describing the problem (for example when <paramref name="housekeeping"/> is <c>null</c>).
        /// </returns>
        /// <remarks>
        /// This method:
        /// <list type="bullet">
        /// <item><description>Creates a record with timestamps from the configured time provider.</description></item>
        /// <item><description>Persists completed chores (if any) and associates them with the created housekeeping entry.</description></item>
        /// <item><description>Marks critical chores as completed based on the created completed-chore records.</description></item>
        /// <item><description>Reloads the persisted entry including completed chores and their related chore navigation data.</description></item>
        /// <item><description>Invalidates prioritized chores cache entries.</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Thrown if <paramref name="cancellationToken"/> is canceled while the operation is in progress.
        /// </exception>
        Task<Result<Housekeeping>> CreateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default);

        Task<Result<Housekeeping>> GetHousekeepingAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Housekeeping>> ListHousekeepingsAsync(CancellationToken cancellationToken = default);
        Task<Result<Housekeeping>> UpdateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default);
        Task<Result> DeleteHousekeepingAsync(int id, CancellationToken cancellationToken = default);
    }
}
