using Bureau;

namespace Watson.Nodes
{
    public sealed class SearchEdgesQuery
    {
        public string? Purpose { get; init; }
        public CursorParameters Cursor { get; init; } = new();
    }
}
