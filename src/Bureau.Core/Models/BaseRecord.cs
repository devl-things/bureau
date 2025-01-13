namespace Bureau.Core.Models
{
    public class BaseRecord : IReference, IAuditable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public RecordStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
