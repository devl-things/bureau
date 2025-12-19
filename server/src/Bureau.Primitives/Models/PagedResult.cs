namespace Bureau
{
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Values { get; }

        public ResultError Error { get; }
        public bool IsSuccess { get; }
        public bool IsError { get { return !IsSuccess; } }

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Count { get; set; }
        public int TotalPages
        {
            get
            {
                if (PageSize <= 0)
                {
                    return 0;
                }

                return (Count + PageSize - 1) / PageSize;
            }
        }
        public bool HasPrevious { get { return Page > 1; } }

        public bool HasNext { get { return Page < TotalPages; } }

        internal PagedResult(IReadOnlyList<T> values, bool isSuccess, ResultError resultError)
        {
            Values = values;
            IsSuccess = isSuccess;
            Error = resultError;
        }
        public PagedResult(IReadOnlyList<T> values) : this(values, true, default)
        {
            Page = 1;
            PageSize = values.Count;
            Count = values.Count;
        }

        public PagedResult(IReadOnlyList<T> values, PagingParameters paging, int totalCount) : this(values, true, default)
        {
            Page = paging.Page;
            PageSize = paging.PageSize;
            Count = totalCount;
        }

        public PagedResult(ResultError error) : this(default!, false, error) { }

        public static implicit operator PagedResult<T>(ResultError error) { return new PagedResult<T>(error); }
    }
}
