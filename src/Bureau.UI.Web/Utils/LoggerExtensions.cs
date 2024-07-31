using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bureau.UI.Web.Utils
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

        public static void Warning(this ILogger logger, string message, string arg)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(message, arg);
            }
        }
    }
}
