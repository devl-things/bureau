namespace Niles.Chores
{
    public class Chore
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ChoreType Type { get; set; }
        public bool IsCrititical { get; set; }
        /// <summary>
        /// If the chore has an open critical chore, this contains the note associated with it.
        /// </summary>
        public string? Note { get; set; }
        /// <summary>
        /// If the chore has an open critical chore, this contains the date when it was created.
        /// </summary>
        public DateTimeOffset? CriticalCreatedAt { get; set; }
        /// <summary>
        /// Every N week(s)
        /// </summary>
        public int WeeklyInterval { get; set; }
    }
}
