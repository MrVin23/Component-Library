using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Client.Components.CustomSelect;

public partial class CustomMultiSelect<TItem> : ComponentBase, IAsyncDisposable where TItem : notnull
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Height of the select element
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "auto";

    /// <summary>
    /// Width of the select element
    /// </summary>
    [Parameter]
    public string Width { get; set; } = "100%";

    /// <summary>
    /// Label text displayed above the select
    /// </summary>
    [Parameter]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Style type of the select component
    /// </summary>
    [Parameter]
    public CustomSelectType SelectType { get; set; } = CustomSelectType.Primary;

    /// <summary>
    /// List of items to display in the select
    /// </summary>
    [Parameter]
    public List<TItem> Items { get; set; } = new();

    /// <summary>
    /// Function to get the display text for an item
    /// </summary>
    [Parameter]
    public Func<TItem, string> DisplayFunc { get; set; } = item => item?.ToString() ?? string.Empty;

    /// <summary>
    /// Function to get the value for an item (used for comparison)
    /// </summary>
    [Parameter]
    public Func<TItem, string> ValueFunc { get; set; } = item => item?.ToString() ?? string.Empty;

    /// <summary>
    /// Currently selected values
    /// </summary>
    [Parameter]
    public List<TItem> SelectedValues { get; set; } = new();

    /// <summary>
    /// Callback when selected values change
    /// </summary>
    [Parameter]
    public EventCallback<List<TItem>> SelectedValuesChanged { get; set; }

    /// <summary>
    /// Placeholder text when no item is selected
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Select items...";

    /// <summary>
    /// Whether the select is disabled
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// Placeholder text for the search input
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Filter...";

    /// <summary>
    /// Text for select all option
    /// </summary>
    [Parameter]
    public string SelectAllText { get; set; } = "Select All";

    private bool _isOpen = false;
    private string _searchText = string.Empty;
    private bool _suppressOpenFromFocus;
    private ElementReference _clickAwayRootRef;
    private ElementReference _inputRef;
    private CustomDropdownPanel? _dropdownPanel;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<CustomMultiSelect<TItem>>? _dotNetObjectRef;
    private bool _isDisposed;
    private bool _suppressClickAway;

    private bool HasSelection => SelectedValues is { Count: > 0 };

    private int SelectedCount => SelectedValues?.Count ?? 0;
    private int TotalCount => Items?.Count ?? 0;
    private bool IsAllSelected => Items != null && Items.Count > 0 && SelectedValues != null && SelectedValues.Count == Items.Count;
    private bool IsIndeterminate => SelectedValues != null && SelectedValues.Count > 0 && !IsAllSelected;

    private string SelectedDisplayText
    {
        get
        {
            if (SelectedValues == null || SelectedValues.Count == 0)
                return Placeholder;
            
            if (SelectedValues.Count == 1)
                return DisplayFunc(SelectedValues[0]);
            
            return $"{SelectedValues.Count} items selected";
        }
    }

    private string InputPlaceholder
    {
        get
        {
            if (_isOpen)
            {
                return SearchPlaceholder;
            }
            return Placeholder;
        }
    }

    private string InputValue
    {
        get
        {
            if (_isOpen)
                return _searchText;

            return SelectedDisplayText == Placeholder ? string.Empty : SelectedDisplayText;
        }
    }

    private List<TItem> FilteredItems
    {
        get
        {
            if (Items == null)
                return new List<TItem>();
                
            if (string.IsNullOrWhiteSpace(_searchText))
                return Items;

            var searchLower = _searchText.ToLowerInvariant();
            return Items.Where(item => DisplayFunc(item).ToLowerInvariant().Contains(searchLower)).ToList();
        }
    }

    private string ContainerCssClass => SelectType switch
    {
        CustomSelectType.Pager => "custom-select-container pager",
        _ => "custom-select-container primary"
    };

    private string LabelCssClass => SelectType switch
    {
        CustomSelectType.Pager => "custom-select-label pager",
        _ => "custom-select-label primary"
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./Components/Selects/CustomMultiSelect.razor.js");
            _dotNetObjectRef = DotNetObjectReference.Create(this);
            await _jsModule.InvokeVoidAsync("initializeClickAway", _clickAwayRootRef, _dotNetObjectRef);
        }
    }

    [JSInvokable]
    public void HandleClickAway()
    {
        if (_isDisposed || _suppressClickAway || !_isOpen)
            return;

        _isOpen = false;
        _searchText = string.Empty;
        StateHasChanged();
    }

    private void OnTriggerMouseDown()
    {
        _suppressOpenFromFocus = true;
    }

    private void OnInputFocus()
    {
        if (Disabled)
            return;

        if (_suppressOpenFromFocus)
        {
            _suppressOpenFromFocus = false;
            return;
        }

        if (!_isOpen)
        {
            _isOpen = true;
            _searchText = string.Empty;
            StateHasChanged();
        }
    }

    private void OnInputClick()
    {
        if (Disabled)
            return;

        _suppressOpenFromFocus = false;
        if (!_isOpen)
        {
            _isOpen = true;
            _searchText = string.Empty;
            StateHasChanged();
        }
    }

    private void OnChevronClick()
    {
        if (Disabled)
            return;

        _suppressOpenFromFocus = false;
        ToggleDropdown();
        StateHasChanged();
    }

    private void OnChevronKeyDown(KeyboardEventArgs e)
    {
        if (Disabled)
            return;

        if (e.Key == "Enter" || e.Key == " ")
        {
            OnChevronClick();
        }
    }

    private void ToggleDropdown()
    {
        if (!Disabled)
        {
            _isOpen = !_isOpen;
            if (_isOpen)
            {
                _searchText = string.Empty;
            }
        }
    }

    private async Task ClearSelection()
    {
        _suppressOpenFromFocus = true;
        _isOpen = false;
        _searchText = string.Empty;
        SelectedValues = [];
        await SelectedValuesChanged.InvokeAsync(SelectedValues);
        await InvokeAsync(StateHasChanged);
    }

    private bool IsItemSelected(TItem item)
    {
        if (SelectedValues == null)
            return false;
            
        return SelectedValues.Any(selected => 
            EqualityComparer<TItem>.Default.Equals(selected, item) ||
            ValueFunc(selected) == ValueFunc(item));
    }

    private async Task ToggleItem(TItem item)
    {
        var newSelectedValues = SelectedValues?.ToList() ?? new List<TItem>();

        if (IsItemSelected(item))
        {
            newSelectedValues.RemoveAll(selected =>
                EqualityComparer<TItem>.Default.Equals(selected, item) ||
                ValueFunc(selected) == ValueFunc(item));
        }
        else
        {
            newSelectedValues.Add(item);
        }

        await UpdateSelectedValuesAsync(newSelectedValues, keepOpen: true);
    }

    private async Task ToggleSelectAll()
    {
        List<TItem> newSelectedValues;

        if (IsAllSelected)
            newSelectedValues = [];
        else
            newSelectedValues = Items?.ToList() ?? [];

        await UpdateSelectedValuesAsync(newSelectedValues, keepOpen: true);
    }

    private async Task UpdateSelectedValuesAsync(List<TItem> newSelectedValues, bool keepOpen)
    {
        _suppressClickAway = true;
        try
        {
            SelectedValues = newSelectedValues;
            await SelectedValuesChanged.InvokeAsync(newSelectedValues);

            if (keepOpen)
                _isOpen = true;

            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            _suppressClickAway = false;
        }
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        if (!_isOpen)
        {
            _isOpen = true;
        }
        StateHasChanged();
    }

    private void OnInputKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            _isOpen = false;
            _searchText = string.Empty;
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        
        if (_jsModule != null && _dotNetObjectRef != null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("dispose", _dotNetObjectRef);
            }
            catch (JSException)
            {
                // JavaScript is disconnected or error occurred, ignore
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled, ignore
            }
            catch (InvalidOperationException)
            {
                // Component already disposed or JS disconnected, ignore
            }
            
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (JSException)
            {
                // JavaScript is disconnected, ignore
            }
            catch (InvalidOperationException)
            {
                // Already disposed, ignore
            }
        }
        
        try
        {
            _dotNetObjectRef?.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Already disposed, ignore
        }
    }
}
