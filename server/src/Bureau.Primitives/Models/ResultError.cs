namespace Bureau
{
    public readonly struct ResultError
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

        public ResultError(string errorMessage)
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
