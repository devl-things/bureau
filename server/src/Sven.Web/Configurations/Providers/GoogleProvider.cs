using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Configuration;
using Sven;

namespace Sven.Configurations.Providers
{
    /// <summary>
    /// Well-known Google OAuth scopes for bureau features.
    /// These are reference values — the active scopes at runtime are loaded from
    /// ExternalProviders:Google:Scopes in configuration.
    /// </summary>
    public static class GoogleScopes
    {
        public static readonly ExternalScope GmailReadonly = new()
        {
            BureauKey = "google.gmail.readonly",
            ProviderScope = "https://www.googleapis.com/auth/gmail.readonly",
            Description = "Read Gmail messages"
        };

        public static readonly ExternalScope CalendarReadonly = new()
        {
            BureauKey = "google.calendar.readonly",
            ProviderScope = "https://www.googleapis.com/auth/calendar.readonly",
            Description = "Read Google Calendar events"
        };

        public static readonly ExternalScope DriveReadonly = new()
        {
            BureauKey = "google.drive.readonly",
            ProviderScope = "https://www.googleapis.com/auth/drive.readonly",
            Description = "Read Google Drive files"
        };
    }

    public static class GoogleProviderExtensions
    {
        private const string ConfigSection = "ExternalProviders:Google:Scopes";

        private static readonly List<ExternalScope> DefaultScopes =
        [
            GoogleScopes.GmailReadonly,
            GoogleScopes.CalendarReadonly,
            GoogleScopes.DriveReadonly,
        ];

        public static SvenProvidersBuilder AddGoogle(this SvenProvidersBuilder builder)
        {
            builder.Services.AddOptions<GoogleOptions>()
                .Bind(builder.Configuration.GetSection("Google"))
                .Validate(
                    o => !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret),
                    "Google configuration is missing or invalid.")
                .ValidateOnStart();

            List<ExternalScope> scopes = builder.Configuration
                .GetSection(ConfigSection)
                .Get<List<ExternalScope>>() ?? DefaultScopes;

            builder.AuthBuilder.AddGoogle(AuthConstants.ExternalSchemes.Google, options =>
            {
                builder.Configuration.Bind("Google", options);
                options.SignInScheme = AuthConstants.AuthenticationSchemes.External;
                foreach (string scope in scopes.Select(s => s.ProviderScope).Distinct())
                    options.Scope.Add(scope);
            });

            builder.RegisterProvider(
                new RegisteredExternalProvider(
                    ProviderKey: AuthConstants.ExternalSchemes.Google,
                    DisplayName: "Google",
                    IconCssClass: "bi-google",
                    CssClass: "btn-google",
                    IsSignInEnabled: true),
                scopes);

            return builder;
        }
    }
}
