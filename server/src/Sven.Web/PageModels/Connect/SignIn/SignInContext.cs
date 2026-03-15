using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven;

namespace Sven.PageModels.Connect.SignIn
{
    public class SignInContext
    {
        private readonly Func<PageResult> _pageFactory;
        private readonly Func<string, RedirectResult> _redirectFactory;
        private readonly Func<int, StatusCodeResult> _statusCodeFactory;

        public SignInMode Mode { get; init; }
        public LoginCredentialsRequest? Credentials { get; init; }
        public string? ErrorMessage { get; set; }
        public HttpRequest Request { get; init; }
        public HttpContext HttpContext { get; init; }

        public SignInContext(
            SignInMode mode,
            Func<PageResult> pageFactory,
            Func<string, RedirectResult> redirectFactory,
            Func<int, StatusCodeResult> statusCodeFactory,
            HttpRequest request,
            HttpContext httpContext,
            LoginCredentialsRequest? credentials = null)
        {
            Mode = mode;
            _pageFactory = pageFactory;
            _redirectFactory = redirectFactory;
            _statusCodeFactory = statusCodeFactory;
            Request = request;
            HttpContext = httpContext;
            Credentials = credentials;
        }

        public PageResult Page()
        {
            return _pageFactory();
        }

        public RedirectResult Redirect(string url)
        {
            return _redirectFactory(url);
        }

        public StatusCodeResult StatusCode(int statusCode)
        {
            return _statusCodeFactory(statusCode);
        }
    }
}
