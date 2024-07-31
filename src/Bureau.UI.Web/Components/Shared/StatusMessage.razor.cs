using Bureau.UI.Web.Components.Account.Managers;
using Bureau.UI.Web.Components.Helpers;
using Microsoft.AspNetCore.Components;

namespace Bureau.UI.Web.Components.Shared
{
    public partial class StatusMessage : ComponentBase
    {
        private string? _messageFromCookie;

        private string StatusMessageClass { get { return string.IsNullOrWhiteSpace(Type) ? "success" : Type; } }

        private string? DisplayMessage => Message ?? _messageFromCookie;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [Parameter]
        public string? Message { get; set; }

        [Parameter]
        public string? Type { get; set; }


        protected override void OnInitialized()
        {
            // TODO #11
            _messageFromCookie = HttpContext.Request.Cookies[CookieNames.IdentityStatusMessage];

            if (_messageFromCookie is not null)
            {
                HttpContext.Response.Cookies.Delete(CookieNames.IdentityStatusMessage);
            }
        }
    }
    public class StatusMessageInput
    {
        public string Message { get; private set; } = string.Empty;
        public string Type { get; private set; } = "success";

        public void SetError(string message)
        {
            Set(message, "danger");
        }

        public void SetWarning(string message) 
        {
            Set(message, "warning");
        }

        public void SetSuccess(string message)
        {
            Set(message, "success");
        }

        public void SetInfo(string message)
        {
            Set(message, "info");
        }

        private void Set(string message, string type) 
        {
            Message = message;
            Type = type;
        }
    }
}
