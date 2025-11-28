using Microsoft.EntityFrameworkCore;
using Niles.Chores;
using Niles.Chores.Abstractions.Services;
using Niles.Chores.Contexts;
using Niles.Chores.Models;

namespace Niles.Chores.Services
{
    internal class ChoreService : IChoreService
    {
        private readonly ChoresContext _context;

        public ChoreService(ChoresContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateChoreAsync(Chore chore, CancellationToken cancellationToken = default)
        {
            if (chore == null) return false;

            var choreDb = new ChoreDb
            {
                Title = chore.Title,
                Description = chore.Description,
                Type = chore.Type,
                WeeklyInterval = chore.WeeklyInterval,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Chores.Add(choreDb);
            await _context.SaveChangesAsync(cancellationToken);

            chore.Id = choreDb.Id;
            return true;
        }

        public async Task<Chore?> GetChoreAsync(int id, CancellationToken cancellationToken = default)
        {
            var choreDb = await _context.Chores.FindAsync(new object[] { id }, cancellationToken);
            if (choreDb == null) return null;

            return MapToChore(choreDb);
        }

        public async Task<IEnumerable<Chore>> ListChoresAsync(CancellationToken cancellationToken = default)
        {
            var choresDb = await _context.Chores.ToListAsync(cancellationToken);
            return choresDb.Select(MapToChore);
        }

        public async Task<bool> UpdateChoreAsync(Chore chore, CancellationToken cancellationToken = default)
        {
            if (chore == null || chore.Id == 0) return false;

            var choreDb = await _context.Chores.FindAsync(new object[] { chore.Id }, cancellationToken);
            if (choreDb == null) return false;

            choreDb.Title = chore.Title;
            choreDb.Description = chore.Description;
            choreDb.Type = chore.Type;
            choreDb.WeeklyInterval = chore.WeeklyInterval;
            choreDb.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteChoreAsync(int id, CancellationToken cancellationToken = default)
        {
            var choreDb = await _context.Chores.FindAsync(new object[] { id }, cancellationToken);
            if (choreDb == null) return false;

            _context.Chores.Remove(choreDb);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static Chore MapToChore(ChoreDb choreDb)
        {
            return new Chore
            {
                Id = choreDb.Id,
                Title = choreDb.Title,
                Description = choreDb.Description,
                Type = choreDb.Type,
                WeeklyInterval = choreDb.WeeklyInterval
            };
        }
    }
}
