namespace Sven.Configurations
{
    public class AuthOptions
    {
        public TimeSpan AuthorizationCodeLifetime { get; set; } = new TimeSpan(0, 5, 0);
    }
}
