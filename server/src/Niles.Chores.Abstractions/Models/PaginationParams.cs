namespace Niles.Chores.Abstractions.Models
{
    public struct PaginationParams
    {
        public string? Search { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PaginationParams(string? search, int page, int pageSize)
        {
            Search = search;
            Page = page;
            PageSize = pageSize;
        }
    }
}

