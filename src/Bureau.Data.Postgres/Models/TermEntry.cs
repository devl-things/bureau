namespace Bureau.Data.Postgres.Models
{
    internal class TermEntry
    {
        /// <summary>
        /// References Record.Id, ensuring uniform handling of term entries.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Title of the term.
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Label associated with the term.
        /// </summary>
        public string Label { get; set; } = null!;

        public Record Record { get; set; } = null!; // Navigation Property
    }
}
