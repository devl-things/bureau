namespace Bureau.Core.Models
{
    [Obsolete("Use NoteDetails instead")]
    public class NoteRecord : BaseRecord
    {
        public string Note { get; set; } = default!;
        public NoteRecord(string id)
        {
            Id = id;
        }
    }
}
