using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Components
{
    public class BureauComponentBase : ComponentBase, IDisposable
    {
        private CancellationTokenSource? _cts;
        protected CancellationToken CancellationToken 
        {
            get 
            {
                return _cts?.Token ?? default;
            }
        }

        [Inject]
        protected BureauNavigationManager Navigation { get; set; } = default!;

        protected override Task OnInitializedAsync()
        {
            _cts = new CancellationTokenSource();
            //Navigation.NavigationManager.LocationChanged += HandleLocationChanged;
            return base.OnInitializedAsync();
        }

        private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            _cts?.Cancel();
        }
        public virtual void Dispose()
        {
            //Navigation.NavigationManager.LocationChanged -= HandleLocationChanged;
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
