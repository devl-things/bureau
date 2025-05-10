using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sven.PageModels.SignIn
{
    public class SignInPageModelFactory : IPageModelFactory<SignInPageModel>
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
                _ => _serviceProvider.GetRequiredService<PlainSignInPageModel>()
            };

            model.PageContext = context;
            return model;
        }
    }
}
