using Bureau;
using System.Diagnostics;
using System.Reflection;

namespace Sven.Extensions
{
    public static class LoggerExtension
    {
        public static void LogResultError(this ILogger logger, ResultError error)
        {
            //TODO ako je result error default to kao da je null
            if (logger is null) throw new ArgumentNullException(nameof(logger));
            if (!logger.IsEnabled(LogLevel.Warning)) return;
            logger.LogWarning("[{Location}] {Error}", GetLocation(), error);
        }

        private static string GetLocation()
        {
            string location = "Unknown location";
            StackFrame? frame = new StackTrace(2, true).GetFrame(0); // skip current frame
            if (frame != null)
            {
                MethodBase? method = frame.GetMethod();
                location = $"{method?.DeclaringType?.FullName}.{method?.Name} in {System.IO.Path.GetFileName(frame.GetFileName())}:{frame.GetFileLineNumber()}";
            }
            return location;
        }

        public static void LogDebugInfo(this ILogger logger, string message)
        {
            if (logger is null) throw new ArgumentNullException(nameof(logger));
            if (!logger.IsEnabled(LogLevel.Debug)) return;
            logger.Log(LogLevel.Debug, "[{Location}] {Message}", GetLocation(), message);
        }

        public static void LogDebugInfo(this ILogger logger, string? message, params object?[] args)
        {
            if (logger is null) throw new ArgumentNullException(nameof(logger));
            if (!logger.IsEnabled(LogLevel.Debug)) return;

            // Build the template: "[{Location}] " + your template (if any)
            string template = string.IsNullOrEmpty(message)
                ? "[{Location}]"
                : "[{Location}] " + message;

            // Prepend the location value to args so it fills {Location}
            if (args is { Length: > 0 })
            {
                object?[] combined = new object?[args.Length + 1];
                combined[0] = GetLocation();
                Array.Copy(args, 0, combined, 1, args.Length);
                logger.Log(LogLevel.Debug, template, combined);
            }
            else
            {
                logger.Log(LogLevel.Debug, template, GetLocation());
            }
        }
    }
}
