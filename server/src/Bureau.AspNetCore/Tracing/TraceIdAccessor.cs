using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Bureau.AspNetCore.Tracing
{
    public static class TraceIdAccessor
    {
        public static string GetTraceId(HttpContext context)
        {
            string? activityId = Activity.Current?.Id;

            if (!string.IsNullOrWhiteSpace(activityId))
            {
                return activityId;
            }

            return context.TraceIdentifier;
        }
    }
}
