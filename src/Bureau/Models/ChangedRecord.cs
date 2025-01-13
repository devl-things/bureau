namespace Bureau.Models
{
    internal class ChangedRecord<T>
    {
        public T Entry { get; set; }
        public bool IsChanged { get; set; } = false;

        public ChangedRecord(T entry)
        {
            Entry = entry;
        }
    }
}
