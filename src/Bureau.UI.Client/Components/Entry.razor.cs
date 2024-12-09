using Bureau.Core.Extensions;
using DomainModels = Bureau.Core.Models;
using Bureau.UI.Attributes;
using Bureau.UI.Client.Events;
using Bureau.UI.Client.Managers;
using Bureau.UI.Client.Models;
using Bureau.UI.Components;
using Bureau.UI.Components.DevLMultiSelect;
using Bureau.UI.Constants;
using Bureau.UI.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Bureau.UI.Client.Components
{
    /// <summary>
    /// 1. new entry
    /// - Id parameter is null
    /// - on every change of anything, change is saved in indexedDb
    /// - entry is saved as draft
    /// - if user clicks back [HandleHeaderEvents] (or MISSING) entry is saved on server
    /// 2. existing entry
    /// - Id parameter is not null
    /// - on every change of anything, change is saved in indexedDb
    /// - entry is saved as active, with "waiting for sync"
    /// - if user clicks back [HandleHeaderEvents] (or MISSING) syncing is "forced"
    /// </summary>
    [Route(BureauUIUris.Entry)]
    [Route($"{BureauUIUris.Entry}/{{{nameof(Id)}}}")]
    [HasHeader]
    public partial class Entry
    {
        private List<TagUI> _tagItems = default!;

        private EntryUI _entry = default!;

        [Inject]
        private ILogger<Entry> _logger { get; set; } = default!;

        [Inject]
        private ITagManager _tagManager { get; set; } = default!;
        [Inject]
        private IEntryManager _entryManager { get; set; } = default!;

        [Inject]
        private IEventMessenger<HeaderNavigationEvent> _headerEventMessenger { get; set; } = default!;

        [Parameter]
        public string? Id { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _tagItems = await _tagManager.GetAllAsync(CancellationToken);
            _headerEventMessenger.OnEventReceived += HandleHeaderEvents;
        }
        protected override async Task OnParametersSetAsync()
        {
            base.OnParametersSetAsync();
            if (string.IsNullOrWhiteSpace(Id))
            {
                _entry = await _entryManager.GetOrAddDraftEntryAsync(CancellationToken);
            }
            else 
            {
                _entry = await _entryManager.GetEntryByIdAsync(Id, CancellationToken);
            }
        }

        /// <summary>
        /// This method represent "Save" event
        /// </summary>
        /// <param name="e"></param>
        private void HandleHeaderEvents(HeaderNavigationEvent e)
        {
            _logger.Warning("Back clicked");
        }

        /*
         * public class DynamicComponentModel
{
    public Type ComponentType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

         * @page "/dynamic-components"

<select class="form-select" @onchange="OnSelectionChange">
    <option value="">Select Component</option>
    <option value="ComponentA">Component A</option>
    <option value="ComponentB">Component B</option>
    <option value="ComponentC">Component C</option>
</select>

<div class="mt-4">
    @foreach (var component in DynamicComponents)
    {
        <DynamicComponent Type="component.ComponentType" Parameters="component.Parameters" />
    }
</div>

@code {
    private List<DynamicComponentModel> DynamicComponents = new List<DynamicComponentModel>();

    private void OnSelectionChange(ChangeEventArgs e)
    {
        var selectedValue = e.Value?.ToString();

        if (!string.IsNullOrEmpty(selectedValue) && !DynamicComponents.Any(c => c.ComponentType.Name == selectedValue))
        {
            DynamicComponents.Add(CreateComponentModel(selectedValue));
        }
    }

    private DynamicComponentModel CreateComponentModel(string componentName) => componentName switch
    {
        "ComponentA" => new DynamicComponentModel
        {
            ComponentType = typeof(ComponentA),
            Parameters = new Dictionary<string, object> { { "Title", "Dynamic Title for A" } }
        },
        "ComponentB" => new DynamicComponentModel
        {
            ComponentType = typeof(ComponentB),
            Parameters = new Dictionary<string, object> { { "Title", "Dynamic Title for B" } }
        },
        "ComponentC" => new DynamicComponentModel
        {
            ComponentType = typeof(ComponentC),
            Parameters = new Dictionary<string, object> { { "Title", "Dynamic Title for C" } }
        },
        _ => throw new InvalidOperationException("Unknown component type")
    };
}

         */
        public Task TagSelectionChangedAsync(DevLMultiSelectEventArgs<TagUI> e)
        {
            return Task.CompletedTask;
        }
        public override void Dispose()
        {
            _headerEventMessenger.OnEventReceived -= HandleHeaderEvents;
            base.Dispose();
        }

        #region test
        protected void OnClick(MouseEventArgs e) 
        {
            Navigation.NavigationManager.NavigateTo($"{BureauUIUris.Entry}/rg", true);
        }
        #endregion
    }
}
