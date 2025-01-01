namespace Bureau.Core
{
    public struct Result
    {
        public ResultError Error { get; }
        public bool IsSuccess { get; }
        public bool IsError { get { return !IsSuccess; } }
        internal Result(bool isSuccess, ResultError resultError)
        {
            IsSuccess = isSuccess;
            Error = resultError;
        }

        public Result() : this(true, default) { }
        public Result(ResultError error) : this(false, error) { }

        public static implicit operator Result(bool data) { return new Result(data, default); }

        public static implicit operator Result(ResultError error) { return new Result(error); }
        public static implicit operator Result(string errorMessage) { return new Result(new ResultError(errorMessage)); }
        public static implicit operator Result(Exception exception) { return new Result(new ResultError(exception)); }
    }
    public struct Result<T> 
    {
        public T Value { get; }
        
        public ResultError Error { get; }
        public bool IsSuccess { get; }
        public bool IsError { get { return !IsSuccess; } }
        internal Result(T value, bool isSuccess, ResultError resultError)
        {
            Value = value;
            IsSuccess = isSuccess;
            Error = resultError;
        }

        public Result(T value) : this(value, true, default) { }
        public Result(ResultError error) : this(default!, false, error) { }

        public static implicit operator Result<T>(T successData) { return new Result<T>(successData); }

        public static implicit operator Result<T>(ResultError error) { return new Result<T>(error); }
        public static implicit operator Result<T>(string errorMessage) { return new Result<T>(new ResultError(errorMessage)); }
        public static implicit operator Result<T>(Exception exception) { return new Result<T>(new ResultError(exception)); }
    }

    public struct PaginatedResult<T>
    {
        public T Value { get; }
        public PaginationMetadata Pagination { get; }
        public ResultError Error { get; }
        public bool IsSuccess { get; }
        public bool IsError { get { return !IsSuccess; } }
        internal PaginatedResult(T value, PaginationMetadata pagination, bool isSuccess, ResultError resultError)
        {
            Value = value;
            Pagination = pagination;
            IsSuccess = isSuccess;
            Error = resultError;
        }

        public PaginatedResult(T value, PaginationMetadata pagination) : this(value, pagination, true, default) { }
        public PaginatedResult(ResultError error) : this(default!, default, false, error) { }

        public static implicit operator PaginatedResult<T>(T successData) { return new PaginatedResult<T>(successData, default); }

        public static implicit operator PaginatedResult<T>(ResultError error) { return new PaginatedResult<T>(error); }
        public static implicit operator PaginatedResult<T>(string errorMessage) { return new PaginatedResult<T>(new ResultError(errorMessage)); }
        public static implicit operator PaginatedResult<T>(Exception exception) { return new PaginatedResult<T>(new ResultError(exception)); }
    }

    public struct ResultError 
    {
        public string ErrorMessage { get; }
        public Exception? Exception { get; }

        public ResultError(string errorMessage) : this()
        {
            ErrorMessage = errorMessage;
        }
        public ResultError(string errorMessage, Exception exception) : this(errorMessage)
        {
            Exception = exception;
        }
        public ResultError(Exception exception) : this(exception.ToString(), exception)
        {
        }

        public override string ToString()
        {
            return $"{ErrorMessage}{(Exception != null ? $"{Environment.NewLine}{Exception.ToString()}" : string.Empty)}";
        }
    }
}
