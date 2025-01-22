using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Bureau.UI.PWA.Layout;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using System.Security.AccessControl;
using Bureau.UI.Attributes;
using Bureau.UI.Managers;

namespace Bureau.UI.PWA
{
    public partial class App : IDisposable
    {
        [Inject]
        private BureauNavigationHistoryManager _historyManager { get; set; } = default!;
        [Inject]
        private BureauNavigationManager _navigation { get; set; } = default!;
        [Inject]
        private AuthenticationStateProvider _authenticationStateProvider { get; set; } = default!;
        private bool _isAuthenticated { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AuthenticationState authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            _isAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;
            _navigation.NavigationManager.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            string currentUrl = _navigation.NavigationManager.ToBaseRelativePath(e.Location);
            _historyManager.Push(currentUrl);
        }
        private Type GetLayout(RouteData routeData)
        {
            bool hasAuthorizeAttribute = routeData.PageType.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Any();

            if (!hasAuthorizeAttribute || !_isAuthenticated)
            {
                // For pages without [Authorize] attribute
                return typeof(PublicLayout);
            }

            bool hasHasHeader = routeData.PageType.GetCustomAttributes(typeof(HasHeaderAttribute), inherit: true).Any();

            return hasHasHeader ? typeof(AuthWithHeaderLayout) : typeof(AuthLayout);
        }

        private IEnumerable<Assembly> GetAdditionalAssemblies()
        {
            List<Assembly> result = new List<Assembly>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) 
            {               
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IUIClientModule).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        result.Add(assembly);
                        break;
                    }
                }
            }
            return result;
        }

        public void Dispose()
        {
            _navigation.NavigationManager.LocationChanged -= OnLocationChanged;
        }
    }
}

