namespace Bureau.Data.Postgres.Models
{
    internal class FlexibleRecord
    {
        /// <summary>
        /// References Record.Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Data type of the flexible record.
        /// </summary>
        public string DataType { get; set; } = null!;

        /// <summary>
        /// JSON data property to store flexible and external record data.
        /// </summary>
        public string Data { get; set; } = null!;

        public Record Record { get; set; } = null!; // Navigation Property
    }
}
