namespace Bureau.Extensions.Time
{
    public static class TimeProviderExtension
    {
        public static DateTimeOffset GetFutureTime(this TimeProvider timeProvider, TimeSpan duration)
        {
            return timeProvider.GetUtcNow().Add(duration);
        }

        public static long GetFutureUnixTimeSeconds(this TimeProvider timeProvider, TimeSpan duration)
        {
            return timeProvider.GetFutureTime(duration).ToUnixTimeSeconds();
        }
    }
}
