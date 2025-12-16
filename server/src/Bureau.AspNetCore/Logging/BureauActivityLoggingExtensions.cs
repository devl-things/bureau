using Microsoft.Extensions.Logging;

namespace Bureau.AspNetCore.Logging
{
    public static class BureauActivityLoggingExtensions
    {
        public static ILoggingBuilder AddBureauActivityTracking(this ILoggingBuilder logging)
        {
            logging.Configure(options =>
            {
                options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId | ActivityTrackingOptions.ParentId |
                    ActivityTrackingOptions.Baggage | ActivityTrackingOptions.Tags;
            });

            return logging;
        }
    }
}
