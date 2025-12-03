namespace Niles.Chores
{
    //TODO [REFACTOR] 
    public struct SearchRequest
    {
        public string? Search { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public SearchRequest(string? search, int page, int pageSize)
        {
            Search = search;
            Page = page;
            PageSize = pageSize;
        }
    }
}

