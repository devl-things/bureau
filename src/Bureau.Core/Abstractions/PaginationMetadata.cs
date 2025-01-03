namespace Bureau.Core
{
    public struct PaginationMetadata
    {
        public int CurrentPage { get; }
        public int PageSize { get; }
        public int TotalItems { get; }

        public PaginationMetadata(int currentPage, int pageSize, int totalItems)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalItems;
        }
    }
}
