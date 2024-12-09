using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Bureau.UI.PWA.Pages
{
    [Authorize]
    public partial class Sample
    {
        private string _message = "Default";

        [SupplyParameterFromForm]
        private FormModel? Model1 { get; set; }

        [CascadingParameter(Name ="context")]
        private AuthenticationState AuthenticationState { get; set; }

        protected override async Task OnInitializedAsync()
        {
            
            Model1 ??= new();
        }
        private async Task Submit1()
        {
            _message = Model1.Message;
        }
        public class FormModel
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
