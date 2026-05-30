using Microsoft.AspNetCore.Components;

namespace Client.Components.GridDisplay;

public partial class GridDisplayRow : ComponentBase
{
    [CascadingParameter(Name = "GridDisplay")]
    public GridDisplay? Parent { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? LabelId { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected bool IsFirstRow { get; private set; }

    protected override void OnInitialized()
    {
        if (Parent is not null)
            IsFirstRow = Parent.RegisterRow() == 0;
    }
}
