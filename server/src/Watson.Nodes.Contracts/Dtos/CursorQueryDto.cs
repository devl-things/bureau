namespace Watson.Nodes.Contracts.Dtos
{
    /// <summary>
    /// Base query parameters for cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// Cursor semantics are exclusive: a request with <see cref="Cursor"/> = 345 returns records strictly after 345.
    /// The server responds with <c>meta.nextCursor</c>, which the client should use as the next <see cref="Cursor"/>.
    /// </remarks>
    public class CursorQueryDto
    {
        /// <summary>
        /// Exclusive cursor position. Results start strictly after this cursor value.
        /// </summary>
        /// <remarks>
        /// Use 0 to start from the beginning. Do not reuse a cursor inclusively; always request the next page using
        /// the returned <c>nextCursor</c>.
        /// </remarks>
        public long Cursor { get; set; }

        /// <summary>
        /// Maximum number of items to return in the response.
        /// </summary>
        /// <remarks>
        /// The server may clamp this value to a safe range.
        /// </remarks>
        public int Limit { get; set; }
    }
}
