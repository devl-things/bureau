using Bureau;

namespace Niles.Chores
{
    public class SearchParameters : PagingParameters
    {
        public string? Search { get; set; }

        public SearchParameters(string? search, int page, int pageSize) : base(page, pageSize)
        {
            Search = search;
        }
    }
}

