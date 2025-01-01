namespace Bureau.Core.Models
{
    public enum RecordStatus
    {
        /// <summary>
        /// Represents an entry that is in progress but not yet finalized
        /// Can be only one per user on server
        /// </summary>
        Draft = 1,
        /// <summary>
        /// Represents a newly created, unconfirmed entry
        /// Happens when entries are from external systems
        /// </summary>
        New = 2,
        /// <summary>
        /// Represents a confirmed and active entry
        /// When a <see cref="Draft"/> is saved 
        /// or <see cref="New"/> is re/viewed entry is <see cref="Active"/>
        /// </summary>
        Active = 3,
        /// <summary>
        /// Represents an entry that is no longer active but retained for reference
        /// </summary>
        Archived = 4,
        // TODO how long is retention period for deleted entries
        /// <summary>
        /// Used for soft deletion, kept for XX times
        /// </summary>
        Deleted = 5
    }
}
