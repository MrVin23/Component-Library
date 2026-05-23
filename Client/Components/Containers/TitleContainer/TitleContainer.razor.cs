using Microsoft.AspNetCore.Components;

namespace Client.Components.Containers.TitleContainer;

public partial class TitleContainer : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    [Parameter]
    public string Title { get; set; } = "";

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public bool ShowDivider { get; set; } = true;
}
