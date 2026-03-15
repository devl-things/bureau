using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sven.Configurations;
using System.Globalization;
using System.Threading.RateLimiting;

namespace Sven.Configurations
{
    public static class RateLimiterServiceCollectionExtensions
    {
        public static IServiceCollection AddSvenRateLimiter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, token) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                    }
                    await context.HttpContext.Response.WriteAsync("Too many requests.", token);
                };

                RateLimitingOptions rl = configuration
                    .GetSection("RateLimiting")
                    .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

                options.AddPolicy("token-endpoint", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rl.TokenPermitLimit,
                            Window = TimeSpan.FromSeconds(rl.TokenWindowSeconds),
                            QueueLimit = 0
                        }));

                options.AddPolicy("authorize-endpoint", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rl.AuthorizePermitLimit,
                            Window = TimeSpan.FromSeconds(rl.AuthorizeWindowSeconds),
                            QueueLimit = 0
                        }));

                options.AddPolicy("signin-endpoint", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rl.SignInPermitLimit,
                            Window = TimeSpan.FromSeconds(rl.SignInWindowSeconds),
                            QueueLimit = 0
                        }));
            });

            return services;
        }
    }
}
