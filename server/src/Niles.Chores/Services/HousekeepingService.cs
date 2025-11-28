using Niles.Chores.Abstractions.Services;

namespace Niles.Chores.Services
{
    public sealed class HousekeepingService : IHousekeepingService
    {
        private readonly List<Housekeeping> _store = new();
        private readonly object _syncRoot = new();
        private int _nextId = 1;

        public Task<bool> CreateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default)
        {
            if (housekeeping == null) return Task.FromResult(false);
            lock (_syncRoot)
            {
                if (!housekeeping.Id.HasValue || housekeeping.Id == 0)
                {
                    housekeeping.Id = _nextId++;
                }
                _store.Add(Clone(housekeeping));
            }
            return Task.FromResult(true);
        }

        public Task<Housekeeping?> GetHousekeepingAsync(int id, CancellationToken cancellationToken = default)
        {
            Housekeeping? found;
            lock (_syncRoot)
            {
                found = _store.FirstOrDefault(x => x.Id == id);
            }
            return Task.FromResult(found is null ? null : Clone(found));
        }

        public Task<IEnumerable<Housekeeping>> ListHousekeepingsAsync(CancellationToken cancellationToken = default)
        {
            List<Housekeeping> snapshot;
            lock (_syncRoot)
            {
                snapshot = _store.Select(Clone).ToList();
            }
            return Task.FromResult<IEnumerable<Housekeeping>>(snapshot);
        }

        public Task<bool> UpdateHousekeepingAsync(Housekeeping housekeeping, CancellationToken cancellationToken = default)
        {
            if (housekeeping == null || !housekeeping.Id.HasValue) return Task.FromResult(false);
            lock (_syncRoot)
            {
                int idx = _store.FindIndex(x => x.Id == housekeeping.Id);
                if (idx == -1) return Task.FromResult(false);
                _store[idx] = Clone(housekeeping);
                return Task.FromResult(true);
            }
        }

        public Task<bool> DeleteHousekeepingAsync(int id, CancellationToken cancellationToken = default)
        {
            lock (_syncRoot)
            {
                int idx = _store.FindIndex(x => x.Id == id);
                if (idx == -1) return Task.FromResult(false);
                _store.RemoveAt(idx);
                return Task.FromResult(true);
            }
        }

        private static Housekeeping Clone(Housekeeping source)
        {
            return new Housekeeping
            {
                Id = source.Id,
                DateTime = source.DateTime,
                Duration = source.Duration,
                Note = source.Note,
                CompletedChoreIds = source.CompletedChoreIds?.ToList() ?? new List<int>()
            };
        }
    }
}
