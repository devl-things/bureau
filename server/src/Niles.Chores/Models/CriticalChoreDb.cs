using Bureau;
namespace Niles.Chores.Models
{
    internal class CriticalChoreDb : IAuditable
    {
        public int Id { get; set; }
        public int ChoreId { get; set; }
        public string? Note { get; set; }
        public int? CompletedChoreId { get; set; }

        public ChoreDb Chore { get; set; } = null!;
        public CompletedChoreDb? CompletedChore { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "niles";
        public DateTimeOffset UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = "niles";
    }
}
