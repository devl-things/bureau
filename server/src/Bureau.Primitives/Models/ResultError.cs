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

        private ResultError(string code, string errorMessage, Exception exception, string logMessage)
        {
            Code = code;
            ErrorMessage = errorMessage;
            LogMessage = logMessage;
            Exception = exception;
        }

        public static ResultError From(ResultError error, string errorMessage)
        {
            return new ResultError(error.Code, errorMessage, error.Exception!, string.Join(";", error.ErrorMessage, error.LogMessage));
        }

        public static ResultError From(string code)
        {
            return new ResultError(code, null!, null!, null!);
        }

        public static ResultError From(string code, string errorMessage)
        {
            return new ResultError(code, errorMessage, null!, null!);
        }
        public static ResultError From(string code, string errorMessage, Exception exception)
        {
            return new ResultError(code, errorMessage, exception, null!);
        }
        public static ResultError From(string code, Exception exception)
        {
            return new ResultError(code, null!, exception, null!);
        }
        public static ResultError From(string code, Exception exception, string logMessage)
        {
            return new ResultError(code, null!, exception, logMessage);
        }
        public static ResultError From(string code, string errorMessage, string logMessage)
        {
            return new ResultError(code, errorMessage, null!, logMessage);
        }
        public static ResultError FromLogMessage(string code, string logMessage)
        {
            return new ResultError(code, null!, null!, logMessage);
        }

        public override string ToString()
        {
            return $"{Code} - {ErrorMessage}{(LogMessage != null ? $"{Environment.NewLine}{LogMessage}" : string.Empty)}{(Exception != null ? $"{Environment.NewLine}{Exception.ToString()}" : string.Empty)}";
        }
    }
}
