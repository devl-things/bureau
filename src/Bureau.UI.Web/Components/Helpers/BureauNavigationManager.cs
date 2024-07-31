using Microsoft.AspNetCore.Components;

namespace Bureau.UI.Web.Components.Helpers
{
    internal class BureauNavigationManager
    {
        public NavigationManager NavigationManager { get; protected set; }
        public BureauNavigationManager(NavigationManager navigationManager)
        {
            NavigationManager = navigationManager;
        }

        internal string GetUriForBasePageAddress(BasePageAddress pageAddress) 
        {
            string uriWithoutQuery = NavigationManager.ToAbsoluteUri(pageAddress.Url).GetLeftPart(UriPartial.Path);
            return NavigationManager.GetUriWithQueryParameters(uriWithoutQuery, pageAddress.QueryParameters);
        }
    }
}
