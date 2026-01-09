namespace Bureau
{
    /// <summary>
    /// Domain cursor parameters used for cursor-based paging.
    /// </summary>
    /// <remarks>
    /// Cursor semantics are exclusive: <see cref="Cursor"/> indicates the last seen position.
    /// Results start strictly after this value.
    /// </remarks>
    public sealed class CursorParameters
    {
        /// <summary>
        /// Minimum allowed limit for a cursor query.
        /// </summary>
        public const int MIN_LIMIT = 1;

        /// <summary>
        /// Maximum allowed limit for a cursor query.
        /// </summary>
        public const int MAX_LIMIT = 500;

        /// <summary>
        /// Exclusive cursor position. Results start strictly after this cursor value.
        /// </summary>
        public long Cursor { get; set; } = 0;

        /// <summary>
        /// Maximum number of records to return.
        /// </summary>
        /// <remarks>
        /// This value is clamped to <see cref="MIN_LIMIT"/> and <see cref="MAX_LIMIT"/>.
        /// </remarks>
        public int Limit { get; set; } = MAX_LIMIT;

        /// <summary>
        /// Creates a new instance with default cursor and limit values.
        /// </summary>
        public CursorParameters()
        {
        }

        /// <summary>
        /// Creates a new instance with the specified cursor and limit.
        /// </summary>
        /// <param name="cursor">Exclusive cursor position.</param>
        /// <param name="limit">Requested maximum number of results.</param>
        public CursorParameters(long cursor, int limit)
        {
            Cursor = cursor;
            Limit = limit;
        }

        /// <summary>
        /// Sets the <see cref="Limit"/> after clamping it to a safe range.
        /// </summary>
        /// <param name="limit">Requested limit.</param>
        public void SetLimit(int limit)
        {
            Limit = Math.Clamp(limit, MIN_LIMIT, MAX_LIMIT);
        }
    }
}
