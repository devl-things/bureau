using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sven.Data;
using Sven.Data.Repositories;
using Sven.Data.Stores;
using Sven.Models;
using Sven.Services;

namespace Sven.Configurations
{
    public static class SvenServiceCollectionExtensions
    {
        public static IServiceCollection AddSvenCore(this IServiceCollection services)
        {
            services.AddScoped<UserRepository>();
            services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<UserRepository>());
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IClientAuthService, ClientAuthService>();
            // SvenTokenProvider constructor is internal (requires IStore<> also internal); register via
            // factory so the DI container does not use reflection-based constructor discovery.
            services.AddScoped<ITokenProvider>(sp => new SvenTokenProvider(
                sp.GetRequiredService<ILogger<SvenTokenProvider>>(),
                sp.GetRequiredService<IOptions<JwtOptions>>(),
                sp.GetRequiredService<RsaSecurityKey>(),
                sp.GetRequiredService<IStore<string, RefreshToken>>(),
                sp.GetRequiredService<TimeProvider>(),
                sp.GetRequiredService<IClientService>(),
                sp.GetRequiredService<IHouseholdService>()
            ));
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IExternalTokenService, ExternalTokenService>();
            services.AddScoped<IHouseholdService, HouseholdService>();
            services.AddTransient<IStartupFilter, EncryptionKeyStartupFilter>();

            // IStore registrations — internal to Sven; Sven.Web never references these types by name
            services.AddSingleton<InMemoryStore<string, AuthCode>>();
            services.AddSingleton<IStore<string, AuthCode>>(sp => sp.GetRequiredService<InMemoryStore<string, AuthCode>>());
            services.AddSingleton<IStore<string, OAuthRequest>, InMemoryStore<string, OAuthRequest>>();
            services.AddSingleton<IStore<string, string>, InMemoryStore<string, string>>();
            services.AddSingleton<IStore<string, UserVerificationCode>, InMemoryStore<string, UserVerificationCode>>();
            services.AddSingleton<IStore<string, RefreshToken>, InMemoryStore<string, RefreshToken>>();

            // IExternalTokenRefresher — internal to Sven; registered here so Sven.Web uses AddSvenCore()
            services.AddScoped<IExternalTokenRefresher, ExternalTokenRefresher>();

            // ITokenExchangeService — TokenExchangeService is internal; register via factory so Sven.Web
            // can resolve ITokenExchangeService without referencing the internal concrete type.
            services.AddScoped<ITokenExchangeService>(sp => new TokenExchangeService(
                sp.GetRequiredService<IClientService>(),
                sp.GetRequiredService<IExternalTokenService>(),
                sp.GetRequiredService<IHouseholdService>(),
                sp.GetRequiredService<IExternalTokenRefresher>(),
                sp.GetRequiredService<IOptions<JwtOptions>>(),
                sp.GetRequiredService<RsaSecurityKey>(),
                sp.GetRequiredService<TimeProvider>(),
                sp.GetRequiredService<ILogger<TokenExchangeService>>()
            ));

            return services;
        }
    }
}
