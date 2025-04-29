namespace Sven.Configurations
{
    public interface ITokenLifetimeOptions
    {
        // can be from 5-60min usually
        public TimeSpan IdTokenLifetime { get; set; }
        public TimeSpan RefreshTokenLifetime { get; set; }
        public TimeSpan AccessTokenLifetime { get; set; }
    }
}
