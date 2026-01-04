using Bureau;
namespace Niles.Chores.Models
{
    internal class HousekeepingDb : IAuditable
    {
        public int Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Note { get; set; }

        public ICollection<CompletedChoreDb> CompletedChores { get; set; } = [];

        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "niles";
        public DateTimeOffset UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = "niles";
    }
}
