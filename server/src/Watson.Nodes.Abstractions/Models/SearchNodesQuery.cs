using Bureau;

namespace Watson.Nodes
{
    public sealed class SearchNodesQuery
    {
        public SearchNodesFilter Filter { get; }
        public CursorParameters Cursor { get; }

        public SearchNodesQuery(SearchNodesFilter filter, CursorParameters cursor)
        {
            Filter = filter;
            Cursor = cursor;
        }
    }
}
