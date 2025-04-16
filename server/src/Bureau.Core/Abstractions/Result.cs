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
        public Result(T? value, bool isSuccess)
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
        public Result(T value) : this(value, true, default) { }
        public Result(ResultError error) : this(default!, false, error) { }

        public static implicit operator Result<T>(T successData) { return new Result<T>(successData); }

        public static implicit operator Result<T>(ResultError error) { return new Result<T>(error); }
        public static implicit operator Result<T>(string errorMessage) { return new Result<T>(new ResultError(errorMessage)); }
        public static implicit operator Result<T>(Exception exception) { return new Result<T>(new ResultError(exception)); }
    }

    public struct ResultError
    {
        /// <summary>
        /// Public message that could be shown to user
        /// </summary>
        public string ErrorMessage { get; }
        /// <summary>
        /// Message that can be logged next to <see cref="ErrorMessage"/> in logs
        /// </summary>
        public string? LogMessage { get; }
        public Exception? Exception { get; }

        public ResultError(string errorMessage) : this()
        {
            ErrorMessage = errorMessage;
        }

        public ResultError(string errorMessage, string logMessage) : this(errorMessage)
        {
            LogMessage = logMessage;
        }
        public ResultError(string errorMessage, Exception exception) : this(errorMessage)
        {
            Exception = exception;
        }
        public ResultError(Exception exception) : this(exception.Message, exception)
        {
        }

        public override string ToString()
        {
            return $"{ErrorMessage}{(LogMessage != null ? $"{Environment.NewLine}{LogMessage}" : string.Empty)}{(Exception != null ? $"{Environment.NewLine}{Exception.ToString()}" : string.Empty)}";
        }
    }
}