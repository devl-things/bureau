using Niles.Chores.Api.Dtos;

namespace Niles.Chores.Api.Factories
{
    public static class SearchParametersFactory
    {
        public const int DEFAULT_PAGE = 1;
        public const int DEFAULT_PAGE_SIZE = 20;
        const int MAX_PAGE_SIZE = 100;
        public static SearchParameters Create(SearchQueryDto dto)
        {
            // Validate pagination parameters
            int page = dto.Page < DEFAULT_PAGE ? DEFAULT_PAGE : dto.Page;
            int pageSize = dto.PageSize;
            if (dto.PageSize < 1) pageSize = DEFAULT_PAGE_SIZE;
            if (pageSize > MAX_PAGE_SIZE) pageSize = MAX_PAGE_SIZE;

            return new SearchParameters(dto.Search, page, pageSize);
        }
    }
}
