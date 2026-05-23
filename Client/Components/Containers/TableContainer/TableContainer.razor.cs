using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Client.Components.Containers.TableContainer;

public partial class TableContainer : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    /// <summary>Optional table section title.</summary>
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public bool ShowTitle { get; set; } = true;

    /// <summary>When false, the search / filter toggle is hidden.</summary>
    [Parameter]
    public bool ShowSearchToggle { get; set; } = true;

    /// <summary>Text shown inside the <see cref="CustomTooltip"/> when hovering the search icon.</summary>
    [Parameter]
    public string SearchTooltip { get; set; } = "Click the icon to show or hide search and filters for this table.";

    /// <summary>Optional rich tooltip body; when set, replaces <see cref="SearchTooltip"/> string.</summary>
    [Parameter]
    public RenderFragment? SearchTooltipContent { get; set; }

    /// <summary>Expanded filter area (inputs, etc.). Shown when the search icon is toggled on.</summary>
    [Parameter]
    public RenderFragment? FilterContent { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Raised after the filter panel is expanded or collapsed.</summary>
    [Parameter]
    public EventCallback<bool> FilterExpandedChanged { get; set; }

    private bool _filterExpanded;
    private string _titleId = string.Empty;
    private string _filterRegionId = string.Empty;

    protected bool HasTitleBlock => ShowTitle && !string.IsNullOrWhiteSpace(Title);

    protected override void OnInitialized()
    {
        var suffix = Guid.NewGuid().ToString("N");
        _titleId = $"table-container-title-{suffix}";
        _filterRegionId = $"table-container-filter-{suffix}";
    }

    private async Task ToggleFilterAsync(MouseEventArgs _)
    {
        _filterExpanded = !_filterExpanded;
        await FilterExpandedChanged.InvokeAsync(_filterExpanded);
        await InvokeAsync(StateHasChanged);
    }
}
