using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.Configuration;
using Sven;

namespace Sven.Configurations.Providers
{
    /// <summary>
    /// Well-known Microsoft Graph scopes for bureau features.
    /// These are reference values — the active scopes at runtime are loaded from
    /// ExternalProviders:Microsoft:Scopes in configuration.
    /// </summary>
    public static class MicrosoftScopes
    {
        public static readonly ExternalScope MailRead = new()
        {
            BureauKey = "microsoft.mail.read",
            ProviderScope = "https://graph.microsoft.com/Mail.Read",
            Description = "Read Outlook mail"
        };

        public static readonly ExternalScope CalendarRead = new()
        {
            BureauKey = "microsoft.calendar.read",
            ProviderScope = "https://graph.microsoft.com/Calendars.Read",
            Description = "Read Outlook calendar events"
        };

        public static readonly ExternalScope FilesRead = new()
        {
            BureauKey = "microsoft.files.read",
            ProviderScope = "https://graph.microsoft.com/Files.Read",
            Description = "Read OneDrive files"
        };
    }

    public static class MicrosoftProviderExtensions
    {
        private const string ConfigSection = "ExternalProviders:Microsoft:Scopes";

        private static readonly List<ExternalScope> DefaultScopes =
        [
            MicrosoftScopes.MailRead,
            MicrosoftScopes.CalendarRead,
            MicrosoftScopes.FilesRead,
        ];

        public static SvenProvidersBuilder AddMicrosoft(this SvenProvidersBuilder builder)
        {
            builder.Services.AddOptions<MicrosoftAccountOptions>()
                .Bind(builder.Configuration.GetSection("Microsoft"))
                .Validate(
                    o => !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret),
                    "Microsoft configuration is missing or invalid.")
                .ValidateOnStart();

            List<ExternalScope> scopes = builder.Configuration
                .GetSection(ConfigSection)
                .Get<List<ExternalScope>>() ?? DefaultScopes;

            builder.AuthBuilder.AddMicrosoftAccount(AuthConstants.ExternalSchemes.Microsoft, options =>
            {
                builder.Configuration.Bind("Microsoft", options);
                options.SignInScheme = AuthConstants.AuthenticationSchemes.External;
                foreach (string scope in scopes.Select(s => s.ProviderScope).Distinct())
                    options.Scope.Add(scope);
            });

            builder.RegisterProvider(
                new RegisteredExternalProvider(
                    ProviderKey: AuthConstants.ExternalSchemes.Microsoft,
                    DisplayName: "Microsoft",
                    IconCssClass: "bi-microsoft",
                    CssClass: "btn-microsoft",
                    IsSignInEnabled: true),
                scopes);

            return builder;
        }
    }
}
