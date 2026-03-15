using System.Diagnostics.CodeAnalysis;

namespace Sven.PageModels.Connect.SignIn
{
    public class SignInHandlerFactory : ISignInHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SignInHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISignInGetHandler GetHandler(SignInMode mode)
        {
            return mode switch
            {
                SignInMode.Pkce => _serviceProvider.GetRequiredService<PkceSignInHandler>(),
                SignInMode.Ticket => _serviceProvider.GetRequiredService<TicketSignInHandler>(),
                _ => _serviceProvider.GetRequiredService<PlainSignInHandler>()
            };
        }

        public bool TryGetPostHandler(SignInMode mode, [NotNullWhen(true)] out ISignInPostHandler? postHandler)
        {
            if (mode == SignInMode.Ticket)
            {
                postHandler = null;
                return false;
            }

            postHandler = mode switch
            {
                SignInMode.Pkce => _serviceProvider.GetRequiredService<PkceSignInHandler>(),
                _ => _serviceProvider.GetRequiredService<PlainSignInHandler>()
            };
            return true;
        }
    }
}
