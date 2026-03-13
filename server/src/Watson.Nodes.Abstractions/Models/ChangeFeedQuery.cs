using Bureau;

namespace Watson.Nodes
{
    public sealed class ChangeFeedQuery
    {
        public CursorParameters Cursor { get; init; } = new CursorParameters();
        public ChangeMode Mode { get; init; }
    }
}
