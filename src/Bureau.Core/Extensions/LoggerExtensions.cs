using Microsoft.Extensions.Logging;

namespace Bureau.Core.Extensions
{
    public static class LoggerExtensions
    {
        public static void Info(this ILogger logger, string message, string arg)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(message, arg);
            }
        }
        public static void Info(this ILogger logger, string message, params object?[] args) 
        {
            if (logger.IsEnabled(LogLevel.Information)) 
            {
                logger.LogInformation(message, args);
            }
        }

        public static void Warning(this ILogger logger, string message) 
        {
            if (logger.IsEnabled(LogLevel.Warning)) 
            {
                logger.LogWarning(message);
            }
        }

        public static void Warning(this ILogger logger, string message, string arg)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(message, arg);
            }
        }

        public static void Warning(this ILogger logger, ResultError error) 
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                string baseMessage = $"Message: {error.ErrorMessage}";
                if (error.Exception != null) 
                {
                    baseMessage = $"{baseMessage}; Exception: {error.Exception.ToString()}";
                }
                logger.LogWarning(baseMessage);
            }
        }
    }
}
