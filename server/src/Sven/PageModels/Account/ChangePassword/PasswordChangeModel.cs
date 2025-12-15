using Sven.Models;
using System.ComponentModel.DataAnnotations;

namespace Sven.PageModels.Account.ChangePassword
{
    public class PasswordChangeModel : IPasswordChangeModel
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Common.MsgFieldIsRequired), ErrorMessageResourceType = typeof(Resources.Common))]
        [Display(Name = nameof(Resources.Common.LblCurrentPassword), ResourceType = typeof(Resources.Common))]
        public string? CurrentPassword { get; set; }
        [Required(ErrorMessageResourceName = nameof(Resources.Common.MsgFieldIsRequired), ErrorMessageResourceType = typeof(Resources.Common))]
        [Display(Name = nameof(Resources.Common.LblNewPassword), ResourceType = typeof(Resources.Common))]
        public string? Password { get; set; }
        [Required(ErrorMessageResourceName = nameof(Resources.Common.MsgFieldIsRequired), ErrorMessageResourceType = typeof(Resources.Common))]
        [Display(Name = nameof(Resources.Common.LblConfirmPassword), ResourceType = typeof(Resources.Common))]
        [Compare(nameof(Password), ErrorMessageResourceName = nameof(Resources.Common.MsgFieldsShouldMatch), ErrorMessageResourceType = typeof(Resources.Common))]
        public string? ConfirmPassword { get; set; }
    }
}
