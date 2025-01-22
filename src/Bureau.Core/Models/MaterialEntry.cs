namespace Bureau.Core.Models
{
    public class MaterialEntry : BaseEntry
    {
        public string FileName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
