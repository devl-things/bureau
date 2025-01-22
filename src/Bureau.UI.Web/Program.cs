using Bureau.Core.Repositories;
using Bureau.Data.Postgres.Configurations;
using Bureau.Recipes.Configurations;
using Bureau.UI.API.Configurations;

namespace Bureau.UI.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            //builder.Services.AddScoped<ITermEntryRepository, MockTermEntryRepository>();
            builder.Services.AddBureauDataPostgres(builder.Configuration);

            builder.Services.AddBureauAPI();

            #region before

            //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            //// Add services to the container.
            //builder.Services.AddRazorComponents()
            //    .AddInteractiveServerComponents();

            //builder.Services.AddHttpContextAccessor();

            //builder.Services.AddCascadingAuthenticationState();

            ////builder.Services.AddScoped<BureauIdentityManager>();
            //builder.Services.AddScoped<BureauRedirectManager>();
            //builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();




            ////builder.Services.AddIdentityServer()
            ////   .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

            ////builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
            ////    .AddBearerToken(IdentityConstants.BearerScheme)
            ////    .AddGoogle(options =>
            ////     {
            ////         options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            ////         options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            ////         options.ClaimActions.MapJsonKey("urn:google:picture", "picture");
            ////         options.Scope.Add("https://www.googleapis.com/auth/calendar");
            ////         options.AccessType = "offline";
            ////         options.SaveTokens = true;
            ////     });
            //builder.Services.AddAuthorizationBuilder();

            //builder.Services.AddAuthentication(options =>
            //    {
            //        options.DefaultScheme = IdentityConstants.ApplicationScheme;
            //        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;

            //        //options.DefaultChallengeScheme = "oidc";
            //        //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //        //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //        //options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        //options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            //    })
            //    //.AddOpenIdConnect("oidc", options => 
            //    //{
            //    //    options.Authority = "https://localhost:7136"; // Blazor Server URL (Identity Provider)
            //    //    options.ClientId = "blazor-client";           // Client ID for the Blazor WASM app
            //    //    options.ClientSecret = "your-client-secret";  // Your secret (can be omitted if using public client)
            //    //    options.ResponseType = "code";
            //    //    options.Scope.Add("openid");
            //    //    options.Scope.Add("profile");
            //    //    options.SaveTokens = true;
            //    //})
            //    //.AddBearerToken()
            //    //.AddIdentityServerJwt()
            //    //.AddJwtBearer(options =>
            //    //{
            //    //    options.TokenValidationParameters = new TokenValidationParameters
            //    //    {
            //    //        ValidateIssuer = true,
            //    //        ValidateAudience = true,
            //    //        ValidateLifetime = true,
            //    //        ValidateIssuerSigningKey = true,
            //    //        ValidIssuer = "https://localhost:7136",
            //    //        ValidAudience = "https://localhost:7157",
            //    //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-signing-key"))
            //    //    };
            //    //})
            //    //.AddOAuth(IdentityConstants.ApplicationScheme, options =>
            //    //{
            //    //    options.AuthorizationEndpoint = "https://localhost:7136/connect/authorize";
            //    //    options.TokenEndpoint = "https://localhost:7136/connect/token";
            //    //    options.ClientId = "BlazorWasmClient";
            //    //    options.ClientSecret = "ClientSecret";
            //    //    options.SaveTokens = true;
            //    //    options.CallbackPath = "/signin-oauth";
            //    //    options.Scope.Add("profile");
            //    //    options.Scope.Add("email");
            //    //})
            //    //.AddCookie()
            //    .AddGoogle(options =>
            //    {
            //        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            //        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            //        options.ClaimActions.MapJsonKey("urn:google:picture", "picture");
            //        options.Scope.Add("https://www.googleapis.com/auth/calendar");
            //        options.AccessType = "offline";
            //        options.SaveTokens = true;
            //    })
            //    .AddIdentityCookies();

            //// https://github.com/dotnet/blazor-samples/blob/main/8.0/BlazorWebAssemblyStandaloneWithIdentity/Backend/Program.cs
            //// Configure app cookie
            ////
            //// The default values, which are appropriate for hosting the Backend and
            //// BlazorWasmAuth apps on the same domain, are Lax and SameAsRequest. 
            //// For more information on these settings, see:
            //// https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity#cross-domain-hosting-same-site-configuration
            //builder.Services.ConfigureApplicationCookie(options =>
            //{
            //    options.Cookie.SameSite = SameSiteMode.Lax;
            //    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            //});



            //builder.Services.AddDatabaseDeveloperPageExceptionFilter();


            ////builder.Services.AddAuthorizationBuilder();



            //builder.Services.AddCors(options => options.AddPolicy(
            //    "wasm",
            //    policy => policy.WithOrigins([builder.Configuration["BackendUrl"] ?? "https://localhost:7136",
            //        builder.Configuration["FrontendUrl"] ?? "https://localhost:7157"])
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .AllowCredentials()));

            //#region Identity
            //builder.Services.AddBureauIdentity(connectionString);
            //#endregion

            //#region Cache
            //builder.Services.AddBureauCache();
            //#endregion

            //#region Modules
            //builder.Services.AddGoogleCalendarModule(options =>
            //{
            //    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            //    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            //    options.ClaimActions.MapJsonKey("urn:google:picture", "picture");
            //    options.Scope.Add("https://www.googleapis.com/auth/calendar");
            //    options.AccessType = "offline";
            //    options.SaveTokens = true;
            //});
            //#endregion

            //builder.Services.AddBureauUIManagers();

            //// builder.Services.AddControllersWithViews();

            #endregion before

            var app = builder.Build();

            app.MapBureauAPI();

            #region before

            //// Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseMigrationsEndPoint();
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}
            //        app.UseCors("wasm");

            //        #region Identity

            //        app.MapAdditionalIdentityEndpoints();
            //        app.UseBureauIdentity();

            //        #endregion

            //        app.UseHttpsRedirection();



            //        app.MapPost("/data-processing-1", ([FromBody] FormModel model) =>
            //Results.Text($"{model.Message.Length} characters"))
            //    .RequireAuthorization();

            //        app.UseDefaultFiles();
            //        app.UseStaticFiles();
            //        app.UseAntiforgery();

            //        //app.MapControllers();

            //        app.MapRazorComponents<App>()
            //            .AddAdditionalAssemblies(typeof(BureauGoogleCalendarUIUris).Assembly)
            //            .AddAdditionalAssemblies(typeof(BureauIdentityUIUris).Assembly)
            //            .AddInteractiveServerRenderMode();

            //        // [react] example
            //        app.MapGet("/api/weatherforecast", (HttpContext httpContext) =>
            //        {
            //            var summaries = new[]
            //            {
            //                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            //            };
            //            var forecast = Enumerable.Range(1, 5).Select(index =>
            //                new
            //                {
            //                    Date = DateTime.Now.AddDays(index),
            //                    TemperatureC = Random.Shared.Next(-20, 55),
            //                    Summary = summaries[Random.Shared.Next(summaries.Length)]
            //                })
            //                .ToArray();
            //            return forecast;
            //        })
            //        .WithName("GetWeatherForecast");
            //        //app.MapFallbackToFile("/react-spa-app/index.html");

            #endregion before

            app.Run();
        }

        class FormModel
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
