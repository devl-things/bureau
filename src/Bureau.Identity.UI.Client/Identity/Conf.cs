using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Identity.UI.Client.Identity
{
    internal class Conf
    {
        //builder.Services.AddTransient<CookieHandler>();
        //builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();


        //builder.Services.AddScoped(sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());
        //builder.Services.AddScoped<IAccountManagement, CookieAuthenticationStateProvider>();

        //    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["FrontendUrl"] ?? "https://localhost:7157") });

        //    builder.Services.AddHttpClient("Auth",
        //        opt => opt.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:7136"))
        //            .AddHttpMessageHandler<CookieHandler>();
    }
}
