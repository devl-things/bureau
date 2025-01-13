namespace Bureau.Core.Models
{
    public class ExternalRecord<T> : FlexibleRecord<T>
    {
        public DateTime LastSync { get; set; }
        public bool Changed { get; set; }

        public ExternalRecord(string id) : base(id)
        {
        }
    }
}
