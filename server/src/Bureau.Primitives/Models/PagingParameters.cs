namespace Bureau
{
    public class PagingParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public int SkipRecords { get { return Page < 1 ? 0 : (Page - 1) * PageSize; } }

        public PagingParameters(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
    }
}
