using Microsoft.Extensions.Logging;

namespace Bureau.Extensions.Logging
{
    public static class BureauLoggerExtensions
    {
        public static void Info(this ILogger logger, string message)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(message);
            }
        }
        public static void Info(this ILogger logger, string message, string? arg1, string? arg2, string? arg3, string? arg4)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(message, arg1, arg2, arg3, arg4);
            }
        }
        public static void Info(this ILogger logger, Exception exception, string message, string? arg1)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(exception, message, arg1);
            }
        }

        public static void LogResultError(this ILogger logger, ResultError error)
        {
            logger.LogResultError(error, traceId: null);
        }

        public static void LogResultError(this ILogger logger, ResultError error, string? traceId)
        {
            if (error.Exception is not null)
            {
                logger.LogError(error.Exception, "Operation failed. ErrorMessage={ErrorMessage}. LogMessage={LogMessage}. Code={Code}. TraceId={TraceId}",
                    error.ErrorMessage, error.LogMessage, error.Code, traceId);
            }
            else
            {
                logger.Info("Operation failed. ErrorMessage={ErrorMessage}. LogMessage={LogMessage}. Code={Code}. TraceId={TraceId}",
                    error.ErrorMessage, error.LogMessage, error.Code, traceId);
            }
        }
    }
}
