namespace Bureau.Core.Configuration
{
    public class BureauOptions
    {
        /// <summary>
        /// Default limit (page size) if not defined
        /// </summary>
        public int DefaultLimit { get; set; } = 100;
        /// <summary>
        /// Maximum limit (page size)
        /// </summary>
        public int MaximumLimit { get; set; } = 100;
        /// <summary>
        /// Size of a batch when processing (crud) records
        /// </summary>
        public int BatchProcessingSize { get; set; } = 100;
    }
}
