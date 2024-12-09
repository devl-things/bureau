using Bureau.Core;

namespace Bureau.Identity.Managers
{
    public interface IEmailManager
    {
        /// <summary>
        /// This API supports the ASP.NET Core Identity infrastructure and is not intended to be used as a general purpose
        /// email abstraction. It should be implemented by the application so the Identity infrastructure can send confirmation emails.
        /// </summary>
        /// <param name="user">The user that is attempting to confirm their email.</param>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="confirmationLink">The link to follow to confirm a user's email. Do not double encode this.</param>
        /// <returns></returns>
        Task SendConfirmationLinkAsync(IUserId user, string email, string confirmationLink);

        /// <summary>
        /// This API supports the ASP.NET Core Identity infrastructure and is not intended to be used as a general purpose
        /// email abstraction. It should be implemented by the application so the Identity infrastructure can send password reset emails.
        /// </summary>
        /// <param name="user">The user that is attempting to reset their password.</param>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="resetLink">The link to follow to reset the user password. Do not double encode this.</param>
        /// <returns></returns>
        Task SendPasswordResetLinkAsync(IUserId user, string email, string resetLink);

        /// <summary>
        /// This API supports the ASP.NET Core Identity infrastructure and is not intended to be used as a general purpose
        /// email abstraction. It should be implemented by the application so the Identity infrastructure can send password reset emails.
        /// </summary>
        /// <param name="user">The user that is attempting to reset their password.</param>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="resetCode">The code to use to reset the user password. Do not double encode this.</param>
        /// <returns></returns>
        Task SendPasswordResetCodeAsync(IUserId user, string email, string resetCode);
    }
}
