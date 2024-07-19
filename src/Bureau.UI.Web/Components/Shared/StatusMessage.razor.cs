using Bureau.UI.Web.Components.Account;
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
            _messageFromCookie = HttpContext.Request.Cookies[IdentityRedirectManager.StatusCookieName];

            if (_messageFromCookie is not null)
            {
                HttpContext.Response.Cookies.Delete(IdentityRedirectManager.StatusCookieName);
            }
        }
    }
    public class StatusMessageInput
    {
        public string Message { get; private set; } = string.Empty;
        public string Type { get; private set; } = "success";

        public void SetError(string message)
        {
            Message = message;
            Type = "danger";
        }

        public void SetSuccess(string message)
        {
            Message = message;
            Type = "success";
        }

        public void SetInfo(string message)
        {
            Message = message;
            Type = "info";
        }
    }
}
