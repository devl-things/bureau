using Microsoft.Extensions.Logging;

namespace Bureau.Extensions.Logging
{
    public static class BureauLoggerExtensions
    {
        public static void Info(this ILogger logger, Exception exception, string message, string arg1)
        {
            if (!logger.IsEnabled(LogLevel.Information))
            {
                return;
            }

            logger.LogInformation(exception, message, arg1);
        }
    }
}
