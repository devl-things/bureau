using Microsoft.Extensions.Localization;
using System.Diagnostics.CodeAnalysis;

namespace Sven.PageModels
{
    public class ErrorTranslations
    {
        public required string InvalidUsernamePassword { get; init; }
        public required string InvalidVerificationCode { get; init; }
        public required string SignUpExistingUser { get; init; }
        public required string UnManageable { get; init; }

        [SetsRequiredMembers]
        public ErrorTranslations(IStringLocalizer<Resources.Errors> localizer)
        {
            InvalidUsernamePassword = localizer[Resources.Errors.MsgInvalidUsernamePassword];
            InvalidVerificationCode = localizer[Resources.Errors.MsgInvalidVerificationCode];
            SignUpExistingUser = localizer[Resources.Errors.MsgSignUpExistingUser];
            UnManageable = localizer[Resources.Errors.MsgUnManageable];
        }
    }
}
