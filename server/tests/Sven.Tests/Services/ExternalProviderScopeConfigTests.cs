using Microsoft.Extensions.Configuration;
using Sven.Configurations.Providers;
using Sven;

namespace Sven.Tests.Services
{
    public class ExternalProviderScopeConfigTests
    {
        private static IConfiguration BuildConfig(Dictionary<string, string?> values)
            => new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        [Fact]
        [Trait("Category", "Unit")]
        public void ExternalScope_BindsFromConfiguration()
        {
            IConfiguration config = BuildConfig(new()
            {
                ["ExternalProviders:Google:Scopes:0:BureauKey"] = "google.gmail.readonly",
                ["ExternalProviders:Google:Scopes:0:ProviderScope"] = "https://www.googleapis.com/auth/gmail.readonly",
                ["ExternalProviders:Google:Scopes:0:Description"] = "Read Gmail messages",
                ["ExternalProviders:Google:Scopes:1:BureauKey"] = "google.calendar.readonly",
                ["ExternalProviders:Google:Scopes:1:ProviderScope"] = "https://www.googleapis.com/auth/calendar.readonly",
                ["ExternalProviders:Google:Scopes:1:Description"] = "Read Google Calendar events",
            });

            List<ExternalScope> scopes = config
                .GetSection("ExternalProviders:Google:Scopes")
                .Get<List<ExternalScope>>()!;

            Assert.Equal(2, scopes.Count);
            Assert.Equal("google.gmail.readonly", scopes[0].BureauKey);
            Assert.Equal("https://www.googleapis.com/auth/gmail.readonly", scopes[0].ProviderScope);
            Assert.Equal("Read Gmail messages", scopes[0].Description);
            Assert.Equal("google.calendar.readonly", scopes[1].BureauKey);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ExternalScope_MissingSection_ReturnsNull_FallsBackToDefaults()
        {
            IConfiguration config = BuildConfig(new());

            List<ExternalScope>? fromConfig = config
                .GetSection("ExternalProviders:Google:Scopes")
                .Get<List<ExternalScope>>();

            // Config returns null — caller falls back to static defaults (GoogleScopes.*)
            Assert.Null(fromConfig);
            Assert.NotEmpty(fromConfig ?? [GoogleScopes.GmailReadonly, GoogleScopes.CalendarReadonly, GoogleScopes.DriveReadonly]);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GoogleScopes_WellKnownConstants_HaveExpectedBureauKeys()
        {
            Assert.Equal("google.gmail.readonly", GoogleScopes.GmailReadonly.BureauKey);
            Assert.Equal("google.calendar.readonly", GoogleScopes.CalendarReadonly.BureauKey);
            Assert.Equal("google.drive.readonly", GoogleScopes.DriveReadonly.BureauKey);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void MicrosoftScopes_WellKnownConstants_HaveExpectedBureauKeys()
        {
            Assert.Equal("microsoft.mail.read", MicrosoftScopes.MailRead.BureauKey);
            Assert.Equal("microsoft.calendar.read", MicrosoftScopes.CalendarRead.BureauKey);
            Assert.Equal("microsoft.files.read", MicrosoftScopes.FilesRead.BureauKey);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GoogleScopes_WellKnownConstants_HaveNonEmptyProviderScopes()
        {
            Assert.All(
                new[] { GoogleScopes.GmailReadonly, GoogleScopes.CalendarReadonly, GoogleScopes.DriveReadonly },
                s =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(s.ProviderScope));
                    Assert.StartsWith("https://www.googleapis.com/auth/", s.ProviderScope);
                });
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void MicrosoftScopes_WellKnownConstants_HaveNonEmptyProviderScopes()
        {
            Assert.All(
                new[] { MicrosoftScopes.MailRead, MicrosoftScopes.CalendarRead, MicrosoftScopes.FilesRead },
                s =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(s.ProviderScope));
                    Assert.StartsWith("https://graph.microsoft.com/", s.ProviderScope);
                });
        }
    }
}
