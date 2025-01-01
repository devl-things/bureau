namespace Bureau.UI.API.Models
{
    public class PaginationData
    {
        public int CurrentPage { get; set; }        // Current page number
        public int PageSize { get; set; }           // Number of items per page
        public int TotalPages { get; set; }         // Total number of pages
        public int TotalItems { get; set; }         // Total number of items
        public bool HasNext { get { return CurrentPage < TotalPages; } }
        public bool HasPrevious { get { return CurrentPage > 1; } }
    }
}
