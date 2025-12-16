using Bureau.Primitives.Errors;

namespace Bureau
{
    public readonly struct ResultError
    {
        /// <summary>
        /// Stable, machine-readable identifier describing the category of the error.
        /// </summary>
        /// <remarks>
        /// The code represents a consistent classification of the failure that can be used
        /// across the system (API responses, background jobs, logs, telemetry).
        /// <para>
        /// Error codes are independent of any transport or protocol and must not encode
        /// HTTP status codes or UI-specific semantics.
        /// </para>
        /// <para>
        /// In most cases, the value should come from predefined constants, for example
        /// from <c>ProblemCodes</c>, to ensure consistency and avoid ad-hoc strings.
        /// </para>
        /// </remarks>
        public string Code { get; }
        /// <summary>
        /// Human-readable error message intended for display to end users.
        /// </summary>
        /// <remarks>
        /// This message represents the public description of the failure and should be
        /// phrased in a clear, non-technical way. Internal diagnostics or sensitive details
        /// should be provided via <see cref="LogMessage"/> or <see cref="Exception"/> instead.
        /// </remarks>
        public string ErrorMessage { get; }
        /// <summary>
        /// Additional diagnostic information intended for logs only.
        /// </summary>
        /// <remarks>
        /// This message may provide technical or contextual details that help with
        /// troubleshooting and debugging. It must not be exposed to end users and
        /// may contain internal or sensitive information.
        /// <para>
        /// When present, this message is typically logged alongside
        /// <see cref="ErrorMessage"/> to provide both a user-safe description and
        /// internal diagnostic context.
        /// </para>
        /// </remarks>
        public string? LogMessage { get; }
        public Exception? Exception { get; }

        public ResultError(string errorMessage)
        {
            Code = ProblemCodes.System.UnexpectedError;
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
        public ResultError(ResultError error, string message)
        {
            Code = ProblemCodes.System.UnexpectedError;
            ErrorMessage = message;
            LogMessage = string.Join(";", error.ErrorMessage, error.LogMessage);
            Exception = error.Exception;
        }

        public override string ToString()
        {
            return $"{ErrorMessage}{(LogMessage != null ? $"{Environment.NewLine}{LogMessage}" : string.Empty)}{(Exception != null ? $"{Environment.NewLine}{Exception.ToString()}" : string.Empty)}";
        }
    }
}
