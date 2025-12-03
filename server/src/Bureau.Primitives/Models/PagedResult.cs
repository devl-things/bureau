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

                return (int)((Count + PageSize - 1) / PageSize);
            }
        }
        public bool HasPrevious { get { return Page > 1; } }

        public bool HasNext { get { return Page < TotalPages; } }

        internal PagedResult(IReadOnlyList<T> value, bool isSuccess, ResultError resultError)
        {
            Values = value;
            IsSuccess = isSuccess;
            Error = resultError;
        }
        public PagedResult(IReadOnlyList<T>? value, bool isSuccess)
        {
            if (isSuccess && value != null)
            {
                Values = value;
                IsSuccess = isSuccess;
            }
            else
            {
                Values = default!;
                IsSuccess = false;
                Error = new ResultError("No value");
            }
        }
        public PagedResult(IReadOnlyList<T> value) : this(value, true, default) { }
        public PagedResult(ResultError error) : this(default!, false, error) { }

        public static implicit operator PagedResult<T>(ResultError error) { return new PagedResult<T>(error); }
        public static implicit operator PagedResult<T>(string errorMessage) { return new PagedResult<T>(new ResultError(errorMessage)); }
        public static implicit operator PagedResult<T>(Exception exception) { return new PagedResult<T>(new ResultError(exception)); }
    }
}
