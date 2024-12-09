using Microsoft.AspNetCore.Components;

namespace Bureau.Identity.UI.Server.Components.Account
{
    public partial class ShowRecoveryCodes
    {
        //TODO where is this used? make it as shared component
        [Parameter]
        public string[] RecoveryCodes { get; set; } = [];

        [Parameter]
        public string? StatusMessage { get; set; }
    }
}
