using System.Diagnostics.CodeAnalysis;

namespace Sven.PageModels.Connect.SignIn
{
    public interface ISignInHandlerFactory
    {
        ISignInGetHandler GetHandler(SignInMode mode);
        bool TryGetPostHandler(SignInMode mode, [NotNullWhen(true)] out ISignInPostHandler? postHandler);
    }
}
