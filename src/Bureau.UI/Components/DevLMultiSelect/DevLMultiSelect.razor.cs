using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Bureau.UI.Constants;
using Bureau.Core;
using Microsoft.Extensions.Logging;
using Bureau.Core.Extensions;

namespace Bureau.UI.Components.DevLMultiSelect
{
    public partial class DevLMultiSelect<T> where T : ISelection
    {
        const string emFactoryMethodNotDefined = "FactoryMethod parameter not defined.";
        const string emItemRemovalNotPossible = "Action to remove an item didn't succeed.";
        const string emItemAdditionNotPossible = "Action to add an item didn't succeed.";

        [Inject]
        private ILogger<DevLMultiSelect<T>> _logger { get; set; } = default!;

        private DevLMultiSelectEventArgs<T> _args;
        private bool _isDropdownVisible = false;

        private string _searchTerm = string.Empty;
        private bool _isAddNewVisible 
        {
            get 
            {
                return !string.IsNullOrWhiteSpace(_searchTerm);
            }
        }
        private List<T> _filteredItems = new List<T>();

        [Parameter]
        public List<T>? SelectedItems { get; set; }
        [Parameter]
        public List<T>? AvailableItems { get; set; }
        [Parameter] 
        public Func<string, Task<List<T>>>? SearchAsync { get; set; }
        [Parameter]
        public Func<string, Result<T>>? FactoryMethod { get; set; }
        [Parameter]
        public EventCallback<DevLMultiSelectEventArgs<T>> OnSelectedChanged { get; set; }
        [Parameter]
        public string Placeholder { get; set; } = "Search or add...";

        protected override Task OnParametersSetAsync()
        {
            if (AvailableItems != null) 
            {
                _filteredItems = AvailableItems;
            }
            _args = new DevLMultiSelectEventArgs<T>(SelectedItems);
            return base.OnParametersSetAsync();
        }
        private void ShowDropdown(MouseEventArgs e)
        {
            _isDropdownVisible = true;
        }

        private void HideDropdown()
        {
            _isDropdownVisible = false;
            StateHasChanged();
        }

        private async Task OnInput(ChangeEventArgs e) 
        {
            _searchTerm = e.Value?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(_searchTerm)) 
            {
                if (SearchAsync != null)
                {
                    _filteredItems = await SearchAsync.Invoke(_searchTerm);
                }
                else 
                {
                    if (AvailableItems != null && AvailableItems.Count > 0) 
                    {
                        _filteredItems = AvailableItems
                            .Where(item => item.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) && !_args.Items.Contains(item))
                            .ToList();
                    }
                }
            }
            StateHasChanged();
        }

        private async Task<bool> TryEventHappenedAsync(EventAction action, T selection) 
        {
            if (_args.TryAction(action, selection)) 
            {
                if (OnSelectedChanged.HasDelegate)
                {
                    await OnSelectedChanged.InvokeAsync(_args);
                }
                return true;
            }
            return false;
        }

        private async Task Selected(MouseEventArgs e, T item)
        {
            if (await TryEventHappenedAsync(EventAction.Add, item)) 
            {
                _searchTerm = string.Empty;
            }
            else
            {
                _logger.Warning(emItemAdditionNotPossible);
            }
            HideDropdown();
        }
        private async Task AddNewItem()
        {
            if (!string.IsNullOrWhiteSpace(_searchTerm)) 
            {
                if (FactoryMethod != null)
                {
                    Result<T> newSelectionResult = FactoryMethod(_searchTerm);
                    if (newSelectionResult.IsSuccess)
                    {
                        if (await TryEventHappenedAsync(EventAction.Add, newSelectionResult.Value)) 
                        {
                            _searchTerm = string.Empty;
                        }
                        else
                        {
                            _logger.Warning(emItemAdditionNotPossible);
                        }
                    }
                    else 
                    {
                        _logger.Warning(newSelectionResult.Error);
                    }
                }
                else
                {
                    _logger.Warning(emFactoryMethodNotDefined);
                }
            }
            HideDropdown();
        }
        private async Task RemoveItem(T item)
        {
            if (!await TryEventHappenedAsync(EventAction.Remove, item))
            {
                _logger.Info(emItemRemovalNotPossible);
            }
            StateHasChanged();
        }
    }
}
