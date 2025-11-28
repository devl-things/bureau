namespace Niles.Chores
{
    public class Chore
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ChoreType Type { get; set; }
        /// <summary>
        /// Every N week(s)
        /// </summary>
        public int WeeklyInterval { get; set; }
    }
}
