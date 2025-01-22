namespace Bureau.Core.Models.Data
{
    public class TermSearchRequest
    {
        public HashSet<string> Terms { get; set; } = default!;

        public TermRequestType RequestType { get; set; } = TermRequestType.Label;
    }
}
