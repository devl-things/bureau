using Microsoft.Extensions.Logging;

namespace Bureau.Core
{
    public static class LoggerExtension
    {
        public static void LogResultError(this ILogger logger, ResultError error)
        {
            //TODO ako je result error default to kao da je null
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(error.ToString());
            }
        }
    }
}
