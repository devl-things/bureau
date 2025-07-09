using Microsoft.Extensions.Localization;

namespace Sven.PageModels.Account
{
    public class AccountTranslations : ILinkedIdentityTranslations, IPasswordChangeTranslations
    {
        public required string BtnCancel { get; init; }
        public required string BtnChangePassword { get; init; }
        public required string BtnUpdate { get; init; }
        public required string LblAccount { get; init; }
        public required string LblAdd { get; init; }
        public required string LblChangePasswordTitle { get; init; }
        public required string LblConnect { get; init; }
        public required string LblConnected { get; init; }
        public required string LblDisconnect { get; init; }
        public required string LblDisconnected { get; init; }
        public required string LblEmail { get; init; }
        public required string LblError { get; init; }
        public required string LblLinkedIdentity { get; init; }
        public required string LblPassword { get; init; }
        public required string LblRemove { get; init; }
        public required string LblManage { get; init; }
        public required string LblUsername { get; init; }
        public required string LblProvider { get; init; }
        public required string LblStatus { get; init; }
        public required string LblActions { get; init; }
        public required string MsgChangePasswordSuccess { get; init; }
        public required string MsgInputIsInvalid { get; init; }
        public required string MsgNoLinkedIdentities { get; init; }
        public AccountTranslations(IStringLocalizer<Resources.Common> localizer, IStringLocalizer<Resources.Pages.Account> accountLocalizer)
        {
            BtnChangePassword = accountLocalizer[Resources.Pages.Account.BtnChangePassword];
            BtnCancel = localizer[Resources.Common.BtnCancel];
            BtnUpdate = localizer[Resources.Common.BtnUpdate];
            LblLinkedIdentity = accountLocalizer[Resources.Pages.Account.Account_ExternalProviders_Subtitle];
            LblAccount = accountLocalizer[Resources.Pages.Account.LblAccount];
            LblAdd = localizer[Resources.Common.LblAdd];
            LblActions = localizer[Resources.Common.LblActions];
            LblChangePasswordTitle = accountLocalizer[Resources.Pages.Account.ChangePassword_Title];
            LblConnect = localizer[Resources.Common.LblConnect];
            LblConnected = accountLocalizer[Resources.Pages.Account.LinkedIdentity_Status_Connected];
            LblDisconnect = localizer[Resources.Common.LblDisconnect];
            LblDisconnected = accountLocalizer[Resources.Pages.Account.LinkedIdentity_Status_Disconnected];
            LblEmail = localizer[Resources.Common.LblEmail];
            LblError = localizer[Resources.Common.LblError];
            LblManage = localizer[Resources.Common.LblManage];
            LblPassword = localizer[Resources.Common.LblPassword];
            LblProvider = localizer[Resources.Common.LblProvider];
            LblRemove = localizer[Resources.Common.LblRemove];
            LblStatus = localizer[Resources.Common.LblStatus];
            LblUsername = localizer[Resources.Common.LblUsername];
            MsgNoLinkedIdentities = localizer[Resources.Common.MsgNoItemsFound];
            MsgInputIsInvalid = localizer[Resources.Common.MsgInputIsInvalid];
            MsgChangePasswordSuccess = accountLocalizer[Resources.Pages.Account.ChangePassword_MsgSuccess];
        }
    }

    public interface ILinkedIdentityTranslations
    {
        public string LblLinkedIdentity { get; }
        public string LblAccount { get; }
        public string LblAdd { get; }
        public string LblConnect { get; }
        public string LblConnected { get; }
        public string LblDisconnect { get; }
        public string LblDisconnected { get; }
        public string LblError { get; }
        public string LblManage { get; }
        public string LblProvider { get; }
        public string LblRemove { get; }
        public string LblStatus { get; }
        public string LblActions { get; }
        public string MsgNoLinkedIdentities { get; }

    }
    public interface IPasswordChangeTranslations
    {
        public string BtnUpdate { get; }
        public string BtnCancel { get; }
        public string LblChangePasswordTitle { get; }
        public string MsgChangePasswordSuccess { get; }
        public string MsgInputIsInvalid { get; }

    }

}
