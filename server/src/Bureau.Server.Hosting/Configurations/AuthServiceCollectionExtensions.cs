using Bureau.Server.Hosting.Dev;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Server.Hosting.Configurations
{
    public static class AuthServiceCollectionExtensions
    {
        public static IServiceCollection AddBureauUiAuth(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);

            ArgumentNullException.ThrowIfNull(configuration);

            AuthOptions options = ConfigureAuthOptions(services, configuration);

            if (options.Mode == AuthMode.None)
            {
                return services;
            }

            if (options.Mode == AuthMode.Dev)
            {
                services
                    .AddAuthentication(authentication =>
                    {
                        authentication.DefaultAuthenticateScheme = AuthConstants.DevUiScheme;
                        authentication.DefaultChallengeScheme = AuthConstants.DevUiScheme;
                    })
                    .AddScheme<AuthenticationSchemeOptions, DevUiAuthenticationHandler>(AuthConstants.DevUiScheme, configureOptions => { });

                services.AddAuthorization();
                return services;
            }

            services
                .AddAuthentication(authentication =>
                {
                    authentication.DefaultScheme = AuthConstants.CookieScheme;
                    authentication.DefaultChallengeScheme = AuthConstants.OidcScheme;
                })
                .AddCookie(AuthConstants.CookieScheme, cookie =>
                {
                    cookie.Cookie.Name = AuthConstants.BureauHostCookieName;
                    cookie.SlidingExpiration = true;
                    // No LoginPath. Challenge will go to OIDC.
                })
                .AddOpenIdConnect(AuthConstants.OidcScheme, oidc =>
                {
                    oidc.SignInScheme = AuthConstants.CookieScheme;

                    oidc.Authority = options.Oidc.Authority;
                    oidc.ClientId = options.Oidc.ClientId;
                    oidc.ClientSecret = options.Oidc.ClientSecret;
                    oidc.CallbackPath = options.Oidc.CallbackPath;

                    oidc.ResponseType = "code";
                    oidc.SaveTokens = true;

                    oidc.Scope.Clear();
                    if (options.Oidc.Scopes != null && options.Oidc.Scopes.Count > 0)
                    {
                        foreach (string scope in options.Oidc.Scopes)
                        {
                            oidc.Scope.Add(scope);
                        }
                    }
                    else
                    {
                        oidc.Scope.Add("openid");
                        oidc.Scope.Add("profile");
                        oidc.Scope.Add("email");
                    }
                });

            services.AddAuthorization();
            return services;
        }

        public static IServiceCollection AddBureauApiAuth(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);

            ArgumentNullException.ThrowIfNull(configuration);

            AuthOptions options = ConfigureAuthOptions(services, configuration);

            if (options.Mode == AuthMode.None)
            {
                return services;
            }

            if (options.Mode == AuthMode.Dev)
            {
                services.AddAuthentication(AuthConstants.DevApiTokenScheme)
                    .AddScheme<AuthenticationSchemeOptions, DevApiTokenAuthenticationHandler>(AuthConstants.DevApiTokenScheme, configureOptions => { });

                services.AddAuthorization();
                return services;
            }

            services
                .AddAuthentication(AuthConstants.JwtBearerScheme)
                .AddJwtBearer(AuthConstants.JwtBearerScheme, jwt =>
                {
                    jwt.Authority = options.Oidc.Authority;
                    jwt.Audience = options.Oidc.Audience;
                    jwt.RequireHttpsMetadata = true;
                });

            services.AddAuthorization();
            return services;
        }


        private static AuthOptions ConfigureAuthOptions(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<AuthOptions>()
                .Bind(configuration.GetSection(AuthOptions.SectionName))
                .ValidateOnStart();

            services.AddSingleton<IDevPrincipalFactory, DevPrincipalFactory>();

            AuthOptions options = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
            return options;
        }
    }
}
