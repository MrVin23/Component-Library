using Microsoft.AspNetCore.Components;

namespace Client.Components.Containers;

public partial class BaseContainer : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>When false, inner content sits flush against the container edges.</summary>
    [Parameter]
    public bool Padding { get; set; } = true;

    /// <summary>When false, no container outline is drawn (see border.css <c>--border-container-*</c> tokens).</summary>
    [Parameter]
    public bool Border { get; set; } = true;

    protected string CssClass
    {
        get
        {
            var classes = new List<string> { "base-container" };
            if (!Padding)
                classes.Add("base-container--no-padding");
            if (!Border)
                classes.Add("base-container--no-border");
            return string.Join(" ", classes);
        }
    }
}
