namespace Bureau.Core.Models
{
    public class ContactEntry : BaseEntry, ITag
    {
        public string Name { get; set; } = default!;
        public string Label { get { return Name; } }

    }
}
