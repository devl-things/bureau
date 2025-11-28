using Niles.Chores.Abstractions.Services;

namespace Niles.Chores.Services
{
    public sealed class ChoreService : IChoreService
    {
        private readonly List<Chore> _store = new();
        private readonly object _syncRoot = new();
        private int _nextId = 1;

        public Task<Chore?> GetChoreAsync(int id, CancellationToken cancellationToken = default)
        {
            Chore? found;
            lock (_syncRoot)
            {
                found = _store.FirstOrDefault(x => x.Id == id);
            }
            return Task.FromResult(found is null ? null : Clone(found));
        }

        public Task<IEnumerable<Chore>> ListChoresAsync(CancellationToken cancellationToken = default)
        {
            List<Chore> snapshot;
            lock (_syncRoot)
            {
                snapshot = _store.Select(Clone).ToList();
            }
            return Task.FromResult<IEnumerable<Chore>>(snapshot);
        }

        public Task<Chore> CreateChoreAsync(Chore chore, CancellationToken cancellationToken = default)
        {
            if (chore == null) throw new ArgumentNullException(nameof(chore));
            
            lock (_syncRoot)
            {
                if (chore.Id == 0)
                {
                    chore.Id = _nextId++;
                }
                else if (_store.Any(x => x.Id == chore.Id))
                {
                    throw new InvalidOperationException($"Chore with ID {chore.Id} already exists.");
                }
                
                var newChore = Clone(chore);
                _store.Add(newChore);
                return Task.FromResult(Clone(newChore));
            }
        }

        public Task<Chore?> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default)
        {
            if (chore == null) throw new ArgumentNullException(nameof(chore));
            if (chore.Id == 0) return Task.FromResult<Chore?>(null);

            lock (_syncRoot)
            {
                int idx = _store.FindIndex(x => x.Id == chore.Id);
                if (idx == -1) return Task.FromResult<Chore?>(null);
                
                _store[idx] = Clone(chore);
                return Task.FromResult<Chore?>(Clone(_store[idx]));
            }
        }

        public Task<bool> DeleteChoreAsync(int id, CancellationToken cancellationToken = default)
        {
            lock (_syncRoot)
            {
                int idx = _store.FindIndex(x => x.Id == id);
                if (idx == -1) return Task.FromResult(false);
                
                _store.RemoveAt(idx);
                return Task.FromResult(true);
            }
        }

        private static Chore Clone(Chore source)
        {
            return new Chore
            {
                Id = source.Id,
                Title = source.Title,
                Description = source.Description,
                Type = source.Type,
                WeeklyInterval = source.WeeklyInterval
            };
        }
    }
}

