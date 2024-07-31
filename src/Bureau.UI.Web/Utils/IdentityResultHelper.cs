using Microsoft.AspNetCore.Identity;

namespace Bureau.UI.Web.Utils
{
    internal static class IdentityResultHelper
    {
        internal static IdentityResult CriticalError() 
        {
            return IdentityResult.Failed(new IdentityError()
            {
                Code = nameof(UIMessages.CriticalError),
                Description = UIMessages.CriticalError
            });
        }

        internal static IdentityResult LoginAlreadyAssociatedWithAccount()
        {
            return IdentityResult.Failed(new IdentityError() 
            {
                Code = nameof(UIMessages.LoginAlreadyAssociatedWithAccount),
                Description = UIMessages.LoginAlreadyAssociatedWithAccount
            });
        }
    }
}
