using Sven.Data;
using Sven;

namespace Sven.Services
{
    public class ExternalTokenRefresher : IExternalTokenRefresher
    {
        private readonly ILogger<ExternalTokenRefresher> _logger;
        private readonly IExternalTokenService _store;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TimeProvider _timeProvider;
        private readonly IConfiguration _configuration;

        public ExternalTokenRefresher(
            ILogger<ExternalTokenRefresher> logger,
            IExternalTokenService store,
            IHttpClientFactory httpClientFactory,
            TimeProvider timeProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _store = store;
            _httpClientFactory = httpClientFactory;
            _timeProvider = timeProvider;
            _configuration = configuration;
        }

        public async Task RefreshAsync(UserExternalToken token, CancellationToken cancellationToken = default)
        {
            if (token.RefreshToken == null)
            {
                _logger.LogWarning("Cannot refresh token for user {UserId} provider {Provider}: no refresh token stored.",
                    token.UserId, token.Provider);
                return;
            }

            (string tokenEndpoint, string clientId, string clientSecret) = GetProviderConfig(token.Provider);

            HttpClient httpClient = _httpClientFactory.CreateClient();
            FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = token.RefreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            });

            HttpResponseMessage response = await httpClient.PostAsync(tokenEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Token refresh failed for user {UserId} provider {Provider}: {Status} {Body}",
                    token.UserId, token.Provider, response.StatusCode, body);
                throw new InvalidOperationException($"Provider {token.Provider} returned {response.StatusCode} on token refresh.");
            }

            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(cancellationToken));
            System.Text.Json.JsonElement root = doc.RootElement;

            string newAccessToken = root.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("access_token missing from refresh response.");
            int expiresIn = root.TryGetProperty("expires_in", out System.Text.Json.JsonElement expiresEl)
                ? expiresEl.GetInt32() : 3600;
            string? newRefreshToken = root.TryGetProperty("refresh_token", out System.Text.Json.JsonElement rtEl)
                ? rtEl.GetString() : null;

            token.AccessToken = newAccessToken;
            token.RefreshToken = newRefreshToken ?? token.RefreshToken;
            token.ExpiresAt = _timeProvider.GetUtcNow().AddSeconds(expiresIn - 60); // 60s buffer
            token.LastRefreshedAt = _timeProvider.GetUtcNow();
            token.RequiresReauthorisation = false;

            await _store.StoreAsync(token, cancellationToken);
            _logger.LogInformation("Refreshed external token for user {UserId} provider {Provider}. New expiry: {ExpiresAt}.",
                token.UserId, token.Provider, token.ExpiresAt);
        }

        private (string tokenEndpoint, string clientId, string clientSecret) GetProviderConfig(string provider)
        {
            return provider.ToLowerInvariant() switch
            {
                "google" => (
                    "https://oauth2.googleapis.com/token",
                    _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google:ClientId not configured."),
                    _configuration["Google:ClientSecret"] ?? throw new InvalidOperationException("Google:ClientSecret not configured.")
                ),
                "microsoft" => (
                    "https://login.microsoftonline.com/common/oauth2/v2.0/token",
                    _configuration["Microsoft:ClientId"] ?? throw new InvalidOperationException("Microsoft:ClientId not configured."),
                    _configuration["Microsoft:ClientSecret"] ?? throw new InvalidOperationException("Microsoft:ClientSecret not configured.")
                ),
                _ => throw new InvalidOperationException($"No token refresh configuration for provider '{provider}'.")
            };
        }
    }
}
