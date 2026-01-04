namespace Watson.Nodes
{
    public sealed class ChangeFeed
    {
        public IReadOnlyList<ChangeEvent> Events { get; }
        public long NextCursor { get; }
        public ChangeMode Mode { get; }

        public ChangeFeed(IReadOnlyList<ChangeEvent> events, long nextCursor, ChangeMode mode)
        {
            Events = events;
            NextCursor = nextCursor;
            Mode = mode;
        }
    }
}
