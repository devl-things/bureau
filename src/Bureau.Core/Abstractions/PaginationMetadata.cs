namespace Bureau.Core
{
    public class PaginationMetadata
    {
        public int Page { get; }
        public int Limit { get; }
        
        public int Offset { get { return Page < 1 ? 0 : (Page - 1)*Limit; } }
        public int TotalItems { get; set; }

        public PaginationMetadata(int page, int limit)
        {
            Page = page;
            Limit = limit;
        }

        public PaginationMetadata() : this(1,1)
        {
            TotalItems = 1;
        }
    }
}
