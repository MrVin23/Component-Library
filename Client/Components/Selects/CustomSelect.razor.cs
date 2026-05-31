using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Client.Components.CustomSelect;

public partial class CustomSelect<TItem> : ComponentBase, IAsyncDisposable where TItem : notnull
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    [Parameter]
    public string Height { get; set; } = "auto";

    [Parameter]
    public string Width { get; set; } = "100%";

    [Parameter]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public CustomSelectType SelectType { get; set; } = CustomSelectType.Primary;

    [Parameter]
    public List<TItem> Items { get; set; } = new();

    [Parameter]
    public Func<TItem, string> DisplayFunc { get; set; } = item => item?.ToString() ?? string.Empty;

    [Parameter]
    public Func<TItem, string> ValueFunc { get; set; } = item => item?.ToString() ?? string.Empty;

    [Parameter]
    public TItem? SelectedValue { get; set; }

    [Parameter]
    public EventCallback<TItem?> SelectedValueChanged { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = "Select...";

    [Parameter]
    public bool Disabled { get; set; }

    private bool _isOpen;
    private bool _suppressOpenFromFocus;
    private ElementReference _triggerRef;
    private ElementReference _inputRef;
    private CustomDropdownPanel? _dropdownPanel;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<CustomSelect<TItem>>? _dotNetObjectRef;
    private bool _isDisposed;

    private bool HasSelection =>
        SelectedValue != null && (Items.Count == 0 || Items.Contains(SelectedValue));

    private string InputValue =>
        HasSelection ? DisplayFunc(SelectedValue!) : string.Empty;

    private string ContainerCssClass => SelectType switch
    {
        CustomSelectType.Pager => "custom-select-container pager",
        _ => "custom-select-container primary"
    };

    private string InputCssClass => SelectType switch
    {
        CustomSelectType.Pager => "custom-select-input pager",
        _ => "custom-select-input primary"
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
                "./Components/Selects/CustomSearchSelect.razor.js");
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
        if (_isDisposed || !_isOpen)
            return;

        _isOpen = false;
        StateHasChanged();
    }

    private void OnTriggerMouseDown() => _suppressOpenFromFocus = true;

    private void OnInputFocus()
    {
        if (Disabled)
            return;

        if (_suppressOpenFromFocus)
        {
            _suppressOpenFromFocus = false;
            return;
        }

        OpenDropdown();
    }

    private void OnInputClick()
    {
        if (Disabled)
            return;

        _suppressOpenFromFocus = false;
        OpenDropdown();
    }

    private void OnChevronClick()
    {
        if (Disabled)
            return;

        _suppressOpenFromFocus = false;
        ToggleDropdown();
    }

    private void OnChevronKeyDown(KeyboardEventArgs e)
    {
        if (Disabled)
            return;

        if (e.Key is "Enter" or " ")
            OnChevronClick();
    }

    private void OnInputKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
            CloseDropdown();
    }

    private void OpenDropdown()
    {
        if (Disabled || _isOpen)
            return;

        _isOpen = true;
        StateHasChanged();
    }

    private void ToggleDropdown()
    {
        if (Disabled)
            return;

        _isOpen = !_isOpen;
        StateHasChanged();
    }

    private void CloseDropdown()
    {
        _isOpen = false;
    }

    private async Task ClearSelection()
    {
        _suppressOpenFromFocus = true;
        _isOpen = false;
        SelectedValue = default;
        await SelectedValueChanged.InvokeAsync(default);
        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectItem(TItem item)
    {
        _suppressOpenFromFocus = true;
        _isOpen = false;
        if (!EqualityComparer<TItem>.Default.Equals(item, SelectedValue))
        {
            SelectedValue = item;
            await SelectedValueChanged.InvokeAsync(item);
        }

        await InvokeAsync(StateHasChanged);
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
            catch (JSException) { }
            catch (TaskCanceledException) { }
            catch (InvalidOperationException) { }

            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (JSException) { }
            catch (InvalidOperationException) { }
        }

        try
        {
            _dotNetObjectRef?.Dispose();
        }
        catch (ObjectDisposedException) { }
    }
}

public enum CustomSelectType
{
    /// <summary>Primary style — standard form select.</summary>
    Primary,

    /// <summary>Pager style — compact select for pager components.</summary>
    Pager
}
