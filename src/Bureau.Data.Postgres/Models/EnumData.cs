namespace Bureau.Data.Postgres.Models
{
    /// <summary>
    /// Table for predefined reference data, such as statuses and types.
    /// </summary>
    public class EnumData
    {
        /// <summary>
        /// Primary Key, used to uniquely identify each reference item (non-autoincremented).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// What type of Enum is this?
        /// </summary>
        public string EnumType { get; set; } = null!;

        /// <summary>
        /// Description of the reference item.
        /// </summary>
        public string Description { get; set; } = null!;
    }
}
