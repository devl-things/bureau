namespace Niles.Chores
{
    public class Housekeeping
    {
        public int Id { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Note { get; set; }
        public List<int> CompletedChoreIds { get; set; } = new List<int>();
        public List<Chore> CompletedChores { get; set; } = new List<Chore>();
    }
}
