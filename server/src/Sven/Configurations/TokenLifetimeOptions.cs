namespace Sven.Configurations
{
    public interface ITokenLifetimeOptions
    {
        // can be from 5-60min usually
        public TimeSpan IdTokenLifetime { get; set; }
        public TimeSpan RefreshTokenLifetime { get; set; }
        public TimeSpan AccessTokenLifetime { get; set; }
    }
    public class TokenLifetimeOptions : ITokenLifetimeOptions
    {
        public TokenLifetimeOptions()
        {

        }
        public TokenLifetimeOptions(ITokenLifetimeOptions options)
        {
            IdTokenLifetime = options.IdTokenLifetime;
            RefreshTokenLifetime = options.RefreshTokenLifetime;
            AccessTokenLifetime = options.AccessTokenLifetime;
        }
        // can be from 5-60min usually
        public TimeSpan IdTokenLifetime { get; set; } = new TimeSpan(0, 5, 0);
        public TimeSpan RefreshTokenLifetime { get; set; } = new TimeSpan(30, 0, 0, 0);
        public TimeSpan AccessTokenLifetime { get; set; } = new TimeSpan(1, 0, 0);
    }
}
