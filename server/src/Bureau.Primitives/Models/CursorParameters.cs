namespace Bureau
{
    public sealed class CursorParameters
    {
        public long Cursor { get; set; } = 0;

        public int Limit { get; set; } = 50;

        public CursorParameters()
        {
        }

        public CursorParameters(long cursor, int limit)
        {
            Cursor = cursor;
            Limit = limit;
        }
    }
}
