using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Bureau.Core
{
    public static class LoggerExtension
    {
        public static void LogResultError(this ILogger logger, ResultError error)
        {
            //TODO ako je result error default to kao da je null
            if (logger.IsEnabled(LogLevel.Warning))
            {
                string location = "Unknown location";
                StackFrame? frame = new StackTrace(1, true).GetFrame(0); // skip current frame
                if (frame != null)
                {
                    MethodBase? method = frame.GetMethod();
                    location = $"{method?.DeclaringType?.FullName}.{method?.Name} in {System.IO.Path.GetFileName(frame.GetFileName())}:{frame.GetFileLineNumber()}";
                }
                logger.LogWarning("[{Location}] {Error}", location, error);
            }
        }
    }
}
