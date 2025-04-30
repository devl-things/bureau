using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.IdentityModel.Tokens;
using Sven.Abstractions.Services;
using Sven.AutoValidation;
using Sven.Configurations;
using Sven.Data.SqlServer.Configurations;
using Sven.Models;
using Sven.Services;
using System.Security.Cryptography;

namespace Sven
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // TODO check this
            builder.WebHost.UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });
            builder.Services.AddOptions<GoogleOptions>().Bind(builder.Configuration.GetSection("Google"))
                .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.ClientId) &&
                    !string.IsNullOrWhiteSpace(options.ClientSecret),
                    "Google configuration is missing or invalid.")
                .ValidateOnStart();

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

            builder.Services.AddOptions<AuthOptions>().Bind(builder.Configuration.GetSection("Auth"))
                .Validate(options => options.AuthorizationCodeLifetime <= TimeSpan.FromMinutes(10))
                .ValidateOnStart();

            builder.Services.AddSingleton<RsaSecurityKey>(provider =>
            {
                RSA rsa = RSA.Create(2048);
                return new RsaSecurityKey(rsa)
                {
                    KeyId = Guid.NewGuid().ToString()
                };
            });

            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddSingleton<IStore<string, AuthCode>, InMemoryStore<string, AuthCode>>();
            builder.Services.AddSingleton<IStore<string, OAuthRequest>, InMemoryStore<string, OAuthRequest>>();
            builder.Services.AddSingleton<IStore<string, RefreshToken>, InMemoryStore<string, RefreshToken>>();
            builder.Services.AddSingleton<AuthCodeProvider>();
            builder.Services.AddSingleton<IUserClaimsProvider, InMemoryUserClaimsProvider>();
            builder.Services.AddScoped<IClientProvider, ClientProvider>();
            builder.Services.AddScoped<ITokenProvider, SvenTokenProvider>();
            builder.Services.AddScoped<OAuthValidationFilter>();
            builder.Services.AddSvenSqlServer(builder.Configuration);


            // Config for CORS (allow React dev server access)
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Cookie + external providers
            builder.Services.AddAuthentication()
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddGoogle(AuthConstants.ExternalSchemes.Google, options =>
            {
                builder.Configuration.Bind("Google", options);
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
            // TODO when I get ClientId and ClientSecret
            //.AddMicrosoftAccount(options =>
            //{
            //    options.ClientId = builder.Configuration["Microsoft:ClientId"];
            //    options.ClientSecret = builder.Configuration["Microsoft:ClientSecret"];
            //    options.CallbackPath = "/signin-microsoft";
            //});

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddRazorPages();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            WebApplication app = builder.Build();

            app.UseStaticFiles(); // if you want CSS
            app.MapRazorPages();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();



            app.Run();
        }
    }
}
