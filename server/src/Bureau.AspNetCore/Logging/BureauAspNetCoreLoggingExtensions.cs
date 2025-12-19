using Bureau.AspNetCore.Tracing;
using Bureau.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bureau.AspNetCore.Logging
{
    public static class BureauAspNetCoreLoggingExtensions
    {
        public static void LogResultError(this ILogger logger, ResultError error, HttpContext httpContext)
        {
            string traceId = TraceIdAccessor.GetTraceId(httpContext);
            logger.LogResultError(error, traceId);
        }
    }
}
