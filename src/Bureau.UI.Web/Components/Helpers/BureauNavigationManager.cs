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

        // TODO make better, save page details or something
        internal PageDetails GetPageDetails() 
        {
            string relativeUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            switch (relativeUrl)
            {
                case UriPages.ForumHome:
                    return new PageDetails("Forum");
                default:
                    return new PageDetails(relativeUrl);
            }
        }
    }
}
