using Bureau.Core;

namespace Bureau.UI.API.Models
{
    public class PaginationData
    {
        public int RequestedPage { get; set; }
        public int PageSize { get; set; }           // Number of items per page
        public int TotalItems { get; set; }         // Total number of items
        public int TotalPages { get { return PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize); } }
        public bool HasNext { get { return RequestedPage < TotalPages; } }
        public bool HasPrevious { get { return RequestedPage > 1; } }

        public PaginationData(PaginationMetadata pagination)
        {
            if (pagination != null)
            {
                RequestedPage = pagination.Page;
                PageSize = pagination.Limit;
                TotalItems = pagination.TotalItems;
            }
        }
    }
}
