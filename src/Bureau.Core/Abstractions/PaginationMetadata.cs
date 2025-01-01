namespace Bureau.Core
{
    public struct PaginationMetadata
    {
        public int CurrentPage { get; }
        public int PageSize { get; }
        public int TotalPages { get; }
        public int TotalItems { get; }

        public PaginationMetadata(int currentPage, int pageSize, int totalPages, int totalItems)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            TotalItems = totalItems;
        }
    }
}
