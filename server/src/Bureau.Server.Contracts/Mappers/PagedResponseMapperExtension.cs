namespace Bureau.Server.Contracts.Mappers
{
    public static class PagedResponseMapperExtension
    {
        public static BureauPagedResponse<TDestination> ToPagedResponse<TSource, TDestination>(this PagedResult<TSource> pagedResult, Func<TSource, TDestination> mapTo)
        {
            return new BureauPagedResponse<TDestination>()
            {
                Data = pagedResult.Values.Select(mapTo),
                Meta = new BureauPagedMeta
                {
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize,
                    Total = pagedResult.Count,
                    TotalPages = pagedResult.TotalPages,
                    HasNext = pagedResult.HasNext,
                    HasPrevious = pagedResult.HasPrevious
                }
            };
        }
    }
}
