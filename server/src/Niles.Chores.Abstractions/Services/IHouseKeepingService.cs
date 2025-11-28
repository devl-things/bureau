namespace Niles.Chores
{
    public interface IHouseKeepingService
    {
        Task<bool> CreateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default);
        Task<Housekeeping?> GetHousekeepingAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Housekeeping>> ListHousekeepingsAsync(CancellationToken cancellationToken = default);
        Task<bool> UpdateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default);
        Task<bool> DeleteHousekeepingAsync(int id, CancellationToken cancellationToken = default);
    }
}
