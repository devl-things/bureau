namespace Niles.Chores.Services
{
    internal class HouseKeepingService : IHouseKeepingService
    {
        private readonly List<Housekeeping> _store = new();

        private int _nextId = 1;

        public Task<bool> CreateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default)
        {
            if (housekeeping == null) return Task.FromResult(false);
            if (!housekeeping.Id.HasValue || housekeeping.Id == 0)
            {
                housekeeping.Id = _nextId++;
            }
            _store.Add(housekeeping);
            return Task.FromResult(true);
        }

        public Task<Housekeeping?> GetHousekeepingAsync(int id, CancellationToken cancellationToken = default)
        {
            Housekeeping? found = _store.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(found);
        }

        public Task<IEnumerable<Housekeeping>> ListHousekeepingsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Housekeeping>>(_store.ToList());
        }

        public Task<bool> UpdateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default)
        {
            if (housekeeping == null || !housekeeping.Id.HasValue) return Task.FromResult(false);
            int idx = _store.FindIndex(x => x.Id == housekeeping.Id);
            if (idx == -1) return Task.FromResult(false);
            _store[idx] = housekeeping;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteHousekeepingAsync(int id, CancellationToken cancellationToken = default)
        {
            int idx = _store.FindIndex(x => x.Id == id);
            if (idx == -1) return Task.FromResult(false);
            _store.RemoveAt(idx);
            return Task.FromResult(true);
        }
    }
}
