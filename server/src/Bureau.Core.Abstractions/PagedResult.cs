namespace Bureau.Core
{
    public struct PagedResult<T>
    {
        public IEnumerable<T> Value { get; }

        public ResultError Error { get; }
        public bool IsSuccess { get; }
        public bool IsError { get { return !IsSuccess; } }
        internal PagedResult(IEnumerable<T> value, bool isSuccess, ResultError resultError)
        {
            Value = value;
            IsSuccess = isSuccess;
            Error = resultError;
        }
        public PagedResult(IEnumerable<T>? value, bool isSuccess)
        {
            if (isSuccess && value != null)
            {
                Value = value;
                IsSuccess = isSuccess;
            }
            else
            {
                Value = default!;
                IsSuccess = false;
                Error = new ResultError("No value");
            }
        }
        public PagedResult(IEnumerable<T> value) : this(value, true, default) { }
        public PagedResult(ResultError error) : this(default!, false, error) { }

        public static implicit operator PagedResult<T>(ResultError error) { return new PagedResult<T>(error); }
        public static implicit operator PagedResult<T>(string errorMessage) { return new PagedResult<T>(new ResultError(errorMessage)); }
        public static implicit operator PagedResult<T>(Exception exception) { return new PagedResult<T>(new ResultError(exception)); }
    }
}
