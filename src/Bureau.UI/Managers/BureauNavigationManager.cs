using Bureau.UI.Constants;
using Bureau.UI.Models;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace Bureau.UI.Managers
{
    public class BureauNavigationManager : IDisposable
    {
        private NavigationManager _navigationManager;
        private BureauNavigationHistoryManager _historyManager;

        public NavigationManager NavigationManager { get { return _navigationManager; } }
        public BureauNavigationManager(NavigationManager navigationManager, BureauNavigationHistoryManager historyManager)
        {
            _navigationManager = navigationManager;
            _historyManager = historyManager;
        }

        public void NavigateBack() 
        {
            string uri = _historyManager.Pop();
            if (string.IsNullOrWhiteSpace(uri)) 
            {
                _navigationManager.NavigateTo(BureauUIUris.Dashboard);
                return;
            }
            _navigationManager.NavigateTo(uri);
        }

        public string GetUriForBasePageAddress(BasePageAddress pageAddress)
        {
            string uriWithoutQuery = _navigationManager.ToAbsoluteUri(pageAddress.Url).GetLeftPart(UriPartial.Path);
            return _navigationManager.GetUriWithQueryParameters(uriWithoutQuery, pageAddress.QueryParameters);
        }

        public void Dispose()
        {
        }
    }
}
