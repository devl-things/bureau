namespace Bureau
{
    public readonly struct Result
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
    }
    public readonly struct Result<T>
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
    }
}