using Sven.Configurations;
using Sven.Pages.Connect;

namespace Sven.PageModels.SignUp
{
    public class SignUpPageModelFactory : IPageModelFactory<SignUpModel, SignUpPageModel>
    {
        private readonly IServiceProvider _serviceProvider;
        public SignUpPageModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public SignUpPageModel CreateModel(string? type, SignUpModel context)
        {
            SignUpPageModel model = type switch
            {
                Endpoints.Connect.ForgotPassword => _serviceProvider.GetRequiredService<ForgotSignUpPageModel>(),
                Endpoints.Connect.SignUp => _serviceProvider.GetRequiredService<PlainSignUpPageModel>(),
                _ => _serviceProvider.GetRequiredService<PlainSignUpPageModel>()
            };

            model.BasePage = context;
            return model;
        }
    }
}
