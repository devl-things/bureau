using Bureau.UI.Client.Events;
using Bureau.UI.Constants;
using Bureau.UI.Events;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;

namespace Bureau.UI.Client.Components
{
    public partial class HeaderNavigation
    {
        [Inject]
        private BureauNavigationManager _navigation { get; set; } = default!;
        [Inject]
        private IEventMessenger<HeaderNavigationEvent> _eventMessenger { get; set; } = default!;

        private void GoBack()
        {
            _eventMessenger.PublishEvent(HeaderNavigationEvent.Instance);
            _navigation.NavigateBack();
        }
    }
}
