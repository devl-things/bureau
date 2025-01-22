using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Bureau.UI.PWA.Identity;
using Bureau.Identity.UI.Client.Configurations;
using Bureau.UI.Configurations;
using Bureau.UI.Client.Configurations;

namespace Bureau.UI.PWA
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddOptions();

            builder.Services.AddBureauUI();
            builder.Services.AddBureauUIClient();

            builder.Services.AddAuthorizationCore();

            builder.Services.AddMockBureauIdentityUIClient();

            

            //builder.Services.AddOidcAuthentication(options =>
            //{
            //    options.ProviderOptions.Authority = "https://localhost:7136"; // Blazor Server's URL (Identity Provider)
            //    options.ProviderOptions.ClientId = "blazor-client";           // Client ID as registered in the server
            //    options.ProviderOptions.ResponseType = "code";                // Use Authorization Code flow
            //    options.ProviderOptions.DefaultScopes.Add("openid");
            //    options.ProviderOptions.DefaultScopes.Add("profile");

            //    // Configure redirect URL after login
            //    options.ProviderOptions.RedirectUri = "https://localhost:7157/authentication/login-callback"; // WASM redirect
            //    options.ProviderOptions.PostLogoutRedirectUri = "https://localhost:7157";
            //});
            //builder.Services.AddOidcAuthentication(options =>
            //{
            //    // Configure your authentication provider options here.
            //    // For more information, see https://aka.ms/blazor-standalone-auth
            //    //builder.Configuration.Bind("Local", options.ProviderOptions);

            //    options.ProviderOptions.Authority = "https://localhost:7136";
            //    options.ProviderOptions.ClientId = "your-client-id";
            //    options.ProviderOptions.ResponseType = "code";
            //    options.ProviderOptions.DefaultScopes.Add("openid");
            //    options.ProviderOptions.DefaultScopes.Add("profile");
            //    options.ProviderOptions.DefaultScopes.Add("api");

            //});

            WebAssemblyHost app = builder.Build();
            await app.RunAsync();
        }
    }
}
