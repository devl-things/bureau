using Bureau.UI.Constants;
using Microsoft.AspNetCore.Components;

namespace Bureau.UI.Components
{
    public partial class StatusMessage
    {
        private string? _messageFromCookie;

        private string StatusMessageClass { get { return string.IsNullOrWhiteSpace(Type) ? "success" : Type; } }

        private string? DisplayMessage => Message ?? _messageFromCookie;

        //[CascadingParameter]
        //private HttpContext HttpContext { get; set; } = default!;

        [Parameter]
        public string? Message { get; set; }

        [Parameter]
        public string? Type { get; set; }


        protected override void OnInitialized()
        {
            // TODO #11
            //_messageFromCookie = HttpContext.Request.Cookies[BureauUICookieNames.IdentityStatusMessage];

            //if (_messageFromCookie is not null)
            //{
            //    HttpContext.Response.Cookies.Delete(BureauUICookieNames.IdentityStatusMessage);
            //}
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
