using Bureau.Core;

namespace Bureau.Identity.Managers
{
    //TODO [Low] [BureauEmailManager]
    public class BureauEmailManager : IEmailManager
    {
        //private readonly InternalEmailManager _internalEmailManager;

        //internal BureauEmailManager(InternalEmailManager internalEmailManager)
        //{
        //    _internalEmailManager = internalEmailManager;
        //}

        public Task SendConfirmationLinkAsync(IUserId user, string email, string confirmationLink)
        {
            //return _internalEmailManager.SendConfirmationLinkAsync(null, email, confirmationLink);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(IUserId user, string email, string resetCode)
        {
            //return _internalEmailManager.SendPasswordResetCodeAsync(null, email, resetCode);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(IUserId user, string email, string resetLink)
        {
            //return _internalEmailManager.SendPasswordResetLinkAsync(null, email, resetLink);
            return Task.CompletedTask;
        }
    }
}
