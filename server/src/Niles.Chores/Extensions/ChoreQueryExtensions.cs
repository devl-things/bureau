using Niles.Chores.Models;

namespace Niles.Chores.Extensions
{
    internal static class ChoreQueryExtensions
    {
        public static IQueryable<ChoreDetail> SelectChoreWithCritical(this IQueryable<ChoreDb> query)
        {
            return query.Select(chore => new ChoreDetail
            {
                Chore = chore,
                OpenCritical = chore.CriticalChores
                    .Where(c => c.CompletedChoreId == null)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CriticalChoreDetail
                    {
                        CreatedAt = c.CreatedAt,
                        Note = c.Note
                    })
                    .FirstOrDefault()
            });
        }
    }
}
