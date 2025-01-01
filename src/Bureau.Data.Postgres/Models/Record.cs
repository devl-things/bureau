namespace Bureau.Data.Postgres.Models
{

    /// <summary>
    /// BaseRecord and BaseEntry are in one table
    /// </summary>
    internal class Record
    {
        /// <summary>
        /// Primary Key, used as the unique identifier for all related entities.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Status of the record
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Timestamp when the record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// User or system that created the record.
        /// </summary>
        public string CreatedBy { get; set; } = null!;

        /// <summary>
        /// Timestamp for the last update.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User or system that last updated the record.
        /// </summary>
        public string UpdatedBy { get; set; } = null!;

        /// <summary>
        /// Provider name for the record.
        /// </summary>
        public string ProviderName { get; set; } = null!;

        /// <summary>
        /// Optional external identifier.
        /// </summary>
        public string? ExternalId { get; set; }
    }


}
