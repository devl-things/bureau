using Bureau;

namespace Niles.Chores.Services
{
    public interface IHouseKeepingService
    {
        Task<Result<Housekeeping>> CreateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default);
        Task<Housekeeping?> GetHousekeepingAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Housekeeping>> ListHousekeepingsAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<Housekeeping>> ListHousekeepingsPagedAsync(SearchParameters pagination, CancellationToken cancellationToken = default);
        Task<Result<Housekeeping>> UpdateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default);
        Task<Result> DeleteHousekeepingAsync(int id, CancellationToken cancellationToken = default);
    }
}
