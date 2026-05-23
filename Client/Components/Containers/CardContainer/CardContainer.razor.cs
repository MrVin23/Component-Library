using Microsoft.AspNetCore.Components;

namespace Client.Components.Containers.CardContainer;

public partial class CardContainer : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Width { get; set; } = "320px";

    [Parameter]
    public string? Height { get; set; } = "320px";

    [Parameter]
    public string? MaxWidth { get; set; } = "320px";

    [Parameter]
    public string? MaxHeight { get; set; } = "320px";

    [Parameter]
    public string? MinWidth { get; set; } = "320px";

    [Parameter]
    public string? MinHeight { get; set; } = "320px";

    [Parameter]
    public string? ContainerCssClass { get; set; }

    [Parameter]
    public CardContainerEnum Color { get; set; } = CardContainerEnum.Primary;

    protected string CssClass =>
        string.Join(" ", new[]
        {
            "card-container",
            $"card-{Color.ToString().ToLower()}",
            ContainerCssClass
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

    protected string Style
    {
        get
        {
            var styles = new List<string>();

            if (!string.IsNullOrWhiteSpace(Width)) styles.Add($"width:{Width};");
            if (!string.IsNullOrWhiteSpace(Height)) styles.Add($"height:{Height};");
            if (!string.IsNullOrWhiteSpace(MaxWidth)) styles.Add($"max-width:{MaxWidth};");
            if (!string.IsNullOrWhiteSpace(MaxHeight)) styles.Add($"max-height:{MaxHeight};");
            if (!string.IsNullOrWhiteSpace(MinWidth)) styles.Add($"min-width:{MinWidth};");
            if (!string.IsNullOrWhiteSpace(MinHeight)) styles.Add($"min-height:{MinHeight};");

            return string.Join(" ", styles);
        }
    }
}