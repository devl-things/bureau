using Niles.Chores.Api.Factories;

namespace Niles.Chores.Api.Dtos
{
    public class SearchQueryDto
    {
        public string? Search { get; set; }
        public int Page { get; set; } = SearchParametersFactory.DEFAULT_PAGE;
        public int PageSize { get; set; } = SearchParametersFactory.DEFAULT_PAGE_SIZE;
    }
}

