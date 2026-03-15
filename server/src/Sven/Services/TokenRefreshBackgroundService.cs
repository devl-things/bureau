using Microsoft.Extensions.Options;
using Sven.Configurations;
using Sven.Data;
using Sven;

namespace Sven.Services
{
    public class TokenRefreshBackgroundService : BackgroundService
    {
        private readonly ILogger<TokenRefreshBackgroundService> _logger;
        private readonly TokenVaultOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;

        public TokenRefreshBackgroundService(
            ILogger<TokenRefreshBackgroundService> logger,
            IOptions<TokenVaultOptions> options,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _options = options.Value;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token refresh background service started. Interval: {IntervalSeconds}s, LeadTime: {LeadTimeMinutes}min.",
                _options.RefreshIntervalSeconds, _options.RefreshLeadTimeMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.RefreshIntervalSeconds), stoppingToken);
                    await RefreshExpiringSoonAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in token refresh background service.");
                }
            }

            _logger.LogInformation("Token refresh background service stopped.");
        }

        private async Task RefreshExpiringSoonAsync(CancellationToken cancellationToken)
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            IExternalTokenService store = scope.ServiceProvider.GetRequiredService<IExternalTokenService>();
            IExternalTokenRefresher refresher = scope.ServiceProvider.GetRequiredService<IExternalTokenRefresher>();
            TimeProvider timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

            DateTimeOffset threshold = timeProvider.GetUtcNow().AddMinutes(_options.RefreshLeadTimeMinutes);
            IReadOnlyList<UserExternalToken> expiring = await store.GetExpiringSoonAsync(threshold, cancellationToken);

            if (expiring.Count == 0) return;

            _logger.LogInformation("Refreshing {Count} external token(s) expiring before {Threshold}.", expiring.Count, threshold);

            foreach (UserExternalToken token in expiring)
            {
                try
                {
                    await refresher.RefreshAsync(token, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to refresh external token for user {UserId} provider {Provider}.",
                        token.UserId, token.Provider);
                    await store.MarkReauthRequiredAsync(token.UserId, token.Provider, token.ExternalAccountId, cancellationToken);
                }
            }
        }
    }
}
