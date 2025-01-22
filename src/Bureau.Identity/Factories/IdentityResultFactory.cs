using Bureau.Identity.Constants;
using Microsoft.AspNetCore.Identity;

namespace Bureau.Identity.Factories
{
    //TODO remove IdentityError usage outside of this project
    internal class IdentityResultFactory
    {
        internal static IdentityResult CriticalError()
        {
            return IdentityResult.Failed(new IdentityError()
            {
                Code = nameof(BureauIdentityMessages.BureauIdentityCriticalError),
                Description = BureauIdentityMessages.BureauIdentityCriticalError
            });
        }

        internal static IdentityResult LoginAlreadyAssociatedWithAccount()
        {
            return IdentityResult.Failed(new IdentityError()
            {
                Code = nameof(BureauIdentityMessages.LoginAlreadyAssociatedWithAccount),
                Description = BureauIdentityMessages.LoginAlreadyAssociatedWithAccount
            });
        }
    }
}
