using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Client.Components.CustomTooltip;

public partial class CustomTooltip : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Bootstrap icon class for the trigger icon (e.g. "bi-info-circle").
    /// </summary>
    [Parameter]
    public string IconClass { get; set; } = "bi-info-circle";

    /// <summary>
    /// Tooltip position relative to the icon: Top, Bottom, Left, Right.
    /// </summary>
    [Parameter]
    public TooltipPosition Position { get; set; } = TooltipPosition.Top;

    /// <summary>
    /// Optional trigger content to wrap (e.g. a button). When set, replaces the default icon trigger.
    /// </summary>
    [Parameter]
    public RenderFragment? TriggerContent { get; set; }

    /// <summary>
    /// Rich content rendered inside the tooltip bubble.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool _visible;
    private ElementReference _wrapperRef;
    private ElementReference _bubbleRef;
    private IJSObjectReference? _jsModule;

    private string PositionClass => Position switch
    {
        TooltipPosition.Top => "tooltip-top",
        TooltipPosition.Bottom => "tooltip-bottom",
        TooltipPosition.Left => "tooltip-left",
        TooltipPosition.Right => "tooltip-right",
        _ => "tooltip-top"
    };

    protected override async Task OnInitializedAsync()
    {
        _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
            "./Components/CustomTooltip/CustomTooltip.razor.js");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_visible && _jsModule is not null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("positionTooltip", _wrapperRef, _bubbleRef, Position.ToString());
            }
            catch (ObjectDisposedException)
            {
                // Component disposed during async
            }
            catch (JSException)
            {
                // DOM not ready
            }
        }
    }

    private async Task OnShowAsync()
    {
        _visible = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnHideAsync()
    {
        _visible = false;
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            await _jsModule.DisposeAsync();
        }
    }
}

public enum TooltipPosition
{
    Top,
    Bottom,
    Left,
    Right
}
