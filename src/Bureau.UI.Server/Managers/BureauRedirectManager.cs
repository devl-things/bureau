using Bureau.UI.Constants;
using Bureau.UI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace Bureau.UI.Managers
{
    //TODO [BureauRedirectManager] no need to have navigation and redirect manager combine in one
    public sealed class BureauRedirectManager
    {
        private BureauNavigationManager _bureauNavigationManager;
        
        private static readonly CookieBuilder StatusCookieBuilder = new()
        {
            SameSite = SameSiteMode.Strict,
            HttpOnly = true,
            IsEssential = true,
            MaxAge = TimeSpan.FromSeconds(5),
        };
        public BureauNavigationManager BureauNavigationManager { get { return _bureauNavigationManager; } }
        public BureauRedirectManager(BureauNavigationManager navigationManager)
        {
            _bureauNavigationManager = navigationManager;
        }

        [DoesNotReturn]
        public void RedirectTo(string? uri)
        {
            uri ??= "";

            // Prevent open redirects.
            if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
            {
                uri = _bureauNavigationManager.NavigationManager.ToBaseRelativePath(uri);
            }

            // During static rendering, NavigateTo throws a NavigationException which is handled by the framework as a redirect.
            // So as long as this is called from a statically rendered Identity component, the InvalidOperationException is never thrown.
            _bureauNavigationManager.NavigationManager.NavigateTo(uri);
            throw new InvalidOperationException($"{nameof(BureauRedirectManager)} can only be used during static rendering.");
        }

        [DoesNotReturn]
        public void RedirectTo(BasePageAddress redirectAddress)
        {
            RedirectTo(_bureauNavigationManager.GetUriForBasePageAddress(redirectAddress));
        }

        [DoesNotReturn]
        public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
        {
            string newUri = _bureauNavigationManager.GetUriForBasePageAddress(new PageAddress(uri, queryParameters));
            RedirectTo(newUri);
        }

        [DoesNotReturn]
        public void RedirectToWithStatus(string uri, string message, HttpContext context)
        {
            //TODO #11
            context.Response.Cookies.Append(BureauUICookieNames.IdentityStatusMessage, message, StatusCookieBuilder.Build(context));
            RedirectTo(uri);
        }

        private string CurrentPath => _bureauNavigationManager.NavigationManager.ToAbsoluteUri(_bureauNavigationManager.NavigationManager.Uri).GetLeftPart(UriPartial.Path);

        [DoesNotReturn]
        public void RedirectToCurrentPage() => RedirectTo(CurrentPath);

        [DoesNotReturn]
        public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
            => RedirectToWithStatus(CurrentPath, message, context);
    }


}
