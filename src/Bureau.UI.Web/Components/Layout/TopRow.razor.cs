using Bureau.UI.Web.Components.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace Bureau.UI.Web.Components.Layout
{
    public partial class TopRow
    {
        private string? _title;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [Inject]
        private BureauNavigationManager _navigationManager { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        protected override Task OnInitializedAsync()
        {
            _title = _navigationManager.GetPageDetails().Title;
            return base.OnInitializedAsync();
        }
    }
}
