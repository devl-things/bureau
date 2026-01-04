namespace Bureau.Server.Contracts.Mappers
{
    public static class CursorResponseMapperExtension
    {
        public static BureauCursorResponse<TDestination> ToCursorResponse<TSource, TDestination>(this CursorResult<TSource> cursorResult, Func<TSource, TDestination> mapTo)
        {
            IReadOnlyList<TDestination> mappedValues = cursorResult.Values.Select(mapTo).ToArray();

            BureauCursorMeta meta = new BureauCursorMeta
            {
                Cursor = cursorResult.Cursor,
                NextCursor = cursorResult.NextCursor,
                HasMore = cursorResult.HasMore,
                Count = mappedValues.Count,
                Mode = cursorResult.Mode,
                Next = cursorResult.Next
            };

            return new BureauCursorResponse<TDestination>
            {
                Data = mappedValues,
                Meta = meta
            };
        }
    }
}
