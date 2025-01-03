using Bureau.Core;

namespace Bureau.UI.API.Models
{
    public class PaginationData
    {
        public int CurrentPage { get; set; }        // Current page number
        public int PageSize { get; set; }           // Number of items per page
        public int TotalItems { get; set; }         // Total number of items
        public int TotalPages { get { return PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize); } }
        public bool HasNext { get { return CurrentPage < TotalPages; } }
        public bool HasPrevious { get { return CurrentPage > 1; } }

        public PaginationData(PaginationMetadata pagination)
        {
            CurrentPage = pagination.CurrentPage;
            PageSize = pagination.PageSize;
            TotalItems = pagination.TotalItems;
        }
    }
}
