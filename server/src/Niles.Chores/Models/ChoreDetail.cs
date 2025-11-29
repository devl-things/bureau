namespace Niles.Chores.Models
{
    internal class ChoreDetail
    {
        public ChoreDb Chore { get; set; } = null!;

        public int Score { get { return (int)Chore.Type * Chore.WeeklyInterval; } }
        public DateTimeOffset? CompletedAt { get; set; }
        public CriticalChoreDetail? OpenCritical { get; set; }
    }
}
