using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Client.Components.CustomSelect;

public partial class CustomSearchSelect<TItem> : ComponentBase, IAsyncDisposable where TItem : notnull
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
    /// Function to get the value for an item
    /// </summary>
    [Parameter]
    public Func<TItem, string> ValueFunc { get; set; } = item => item?.ToString() ?? string.Empty;

    /// <summary>
    /// Currently selected value
    /// </summary>
    [Parameter]
    public TItem? SelectedValue { get; set; }

    /// <summary>
    /// Callback when selected value changes
    /// </summary>
    [Parameter]
    public EventCallback<TItem?> SelectedValueChanged { get; set; }

    /// <summary>
    /// Placeholder text when no item is selected
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Select...";

    /// <summary>
    /// Whether the select is disabled
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// Placeholder text for the search input
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search...";

    private bool _isOpen = false;
    private string _searchText = string.Empty;
    private bool _suppressOpenFromFocus;
    private ElementReference _triggerRef;
    private ElementReference _inputRef;
    private CustomDropdownPanel? _dropdownPanel;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<CustomSearchSelect<TItem>>? _dotNetObjectRef;
    private bool _isSelectingItem = false;
    private bool _isDisposed = false;

    private string SelectedDisplayText
    {
        get
        {
            if (SelectedValue == null)
                return Placeholder;
            
            // Check if the selected value is actually in the Items list
            // This handles cases where a sentinel value (like -1) is used to represent "no selection"
            if (Items == null || Items.Count == 0 || !Items.Contains(SelectedValue))
                return Placeholder;
            
            return DisplayFunc(SelectedValue);
        }
    }

    private string InputPlaceholder
    {
        get
        {
            if (_isOpen)
            {
                // When open, show selected text as placeholder if there's a selection
                if (SelectedValue != null && (Items == null || Items.Count == 0 || Items.Contains(SelectedValue)))
                {
                    return DisplayFunc(SelectedValue);
                }
                return SearchPlaceholder;
            }
            // When closed, show normal placeholder
            return Placeholder;
        }
    }

    private string InputValue
    {
        get
        {
            if (_isOpen)
            {
                if (!string.IsNullOrEmpty(_searchText))
                    return _searchText;

                if (SelectedValue != null && (Items == null || Items.Count == 0 || Items.Contains(SelectedValue)))
                    return DisplayFunc(SelectedValue);

                return string.Empty;
            }

            if (SelectedValue != null && (Items == null || Items.Count == 0 || Items.Contains(SelectedValue)))
                return DisplayFunc(SelectedValue);

            return string.Empty;
        }
    }

    private List<TItem> FilteredItems
    {
        get
        {
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
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Selects/CustomSearchSelect.razor.js");
            _dotNetObjectRef = DotNetObjectReference.Create(this);
            await _jsModule.InvokeVoidAsync("initializeClickAway", _triggerRef, _dotNetObjectRef);
        }

        if (_jsModule != null && _dotNetObjectRef != null)
        {
            await _jsModule.InvokeVoidAsync(
                "syncClickAwayPanel",
                _dotNetObjectRef,
                _isOpen,
                _isOpen && _dropdownPanel != null ? _dropdownPanel.DropdownPanelRef : default);
        }
    }

    [JSInvokable]
    public void HandleClickAway()
    {
        // Check if component is disposed before trying to update state
        if (_isDisposed)
            return;

        if (_isOpen)
        {
            _isOpen = false;
            _searchText = string.Empty;
            StateHasChanged();
        }
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
            _isSelectingItem = false;
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
            _isSelectingItem = false;
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

    private async Task OnInputBlur()
    {
        // Delay closing to allow click events on dropdown items to fire
        await Task.Delay(200);
        if (!_isSelectingItem)
        {
            _isOpen = false;
            _searchText = string.Empty;
            await InvokeAsync(StateHasChanged);
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
        _isSelectingItem = true;
        _suppressOpenFromFocus = true;
        _isOpen = false;
        _searchText = string.Empty;
        SelectedValue = default(TItem);
        await SelectedValueChanged.InvokeAsync(default(TItem));
        _isSelectingItem = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectItem(TItem item)
    {
        _isSelectingItem = true;
        _suppressOpenFromFocus = true;
        _isOpen = false;
        _searchText = string.Empty;
        if (!EqualityComparer<TItem>.Default.Equals(item, SelectedValue))
        {
            SelectedValue = item;
            await SelectedValueChanged.InvokeAsync(item);
        }
        _isSelectingItem = false;
        await InvokeAsync(StateHasChanged);
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
