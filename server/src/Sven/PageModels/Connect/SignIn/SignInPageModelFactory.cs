using Microsoft.AspNetCore.Mvc.RazorPages;

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
                PageModelTypes.SignIn.Pkce => _serviceProvider.GetRequiredService<PkceSignInPageModel>(),
                PageModelTypes.SignIn.Plain => _serviceProvider.GetRequiredService<PlainSignInPageModel>(),
                PageModelTypes.SignIn.Ticket => _serviceProvider.GetRequiredService<TicketSignInPageModel>(),
                _ => _serviceProvider.GetRequiredService<PlainSignInPageModel>()
            };

            model.PageContext = context;
            return model;
        }
    }
}
