namespace Bureau
{
    public sealed class CursorResult<T>
    {
        public IReadOnlyList<T> Values { get; }

        public ResultError Error { get; }
        public bool IsSuccess { get; }
        public bool IsError { get { return !IsSuccess; } }

        public long Cursor { get; }
        public long NextCursor { get; }
        public bool HasMore { get; }

        public int Count { get { return Values.Count; } }

        internal CursorResult(IReadOnlyList<T> values, long cursor, long nextCursor,
            bool hasMore, bool isSuccess, ResultError resultError)
        {
            Values = values;
            Cursor = cursor;
            NextCursor = nextCursor;
            HasMore = hasMore;
            IsSuccess = isSuccess;
            Error = resultError;
        }

        public CursorResult(IReadOnlyList<T> values, CursorParameters parameters, long nextCursor, bool hasMore)
            : this(values, parameters.Cursor, nextCursor, hasMore, true, default)
        {
        }

        public CursorResult(ResultError error) : this(Array.Empty<T>(), 0, 0, false, false, error)
        {
        }

        public static implicit operator CursorResult<T>(ResultError error)
        {
            return new CursorResult<T>(error);
        }
    }
}
