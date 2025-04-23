namespace Sven.Configurations
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        // can be from 5-60min usually
        public TimeSpan IdTokenLifetime { get; set; } = new TimeSpan(0, 5, 0);
        public TimeSpan RefreshTokenLifetime { get; set; } = new TimeSpan(30, 0, 0, 0);
        public TimeSpan AccessTokenLifetime { get; set; } = new TimeSpan(1, 0, 0);
    }
}
