using Bureau;
namespace Niles.Chores.Models
{
    internal class ChoreDb : IAuditable
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ChoreType Type { get; set; }
        public int WeeklyInterval { get; set; }

        public ICollection<CompletedChoreDb> CompletedChores { get; set; } = [];
        public ICollection<CriticalChoreDb> CriticalChores { get; set; } = [];

        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "niles";
        public DateTimeOffset UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = "niles";
    }
}
