using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven.Configurations;

namespace Sven.PageModels.Connect.SignIn
{
    public class SignInPageModelFactory : IPageModelFactory<PageContext, SignInPageModel>
    {
        private readonly IServiceProvider _serviceProvider;
        public SignInPageModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public SignInPageModel CreateModel(string? type, PageContext context)
        {
            SignInPageModel model = type switch
            {
                Modes.Connect.SignIn.Pkce => _serviceProvider.GetRequiredService<PkceSignInPageModel>(),
                Modes.Connect.SignIn.Plain => _serviceProvider.GetRequiredService<PlainSignInPageModel>(),
                Modes.Connect.SignIn.Ticket => _serviceProvider.GetRequiredService<TicketSignInPageModel>(),
                _ => _serviceProvider.GetRequiredService<PlainSignInPageModel>()
            };

            model.PageContext = context;
            return model;
        }
    }
}
