using Microsoft.AspNetCore.Components;

namespace Client.Components.Breadcrumbs;

/// <summary>One segment in <see cref="BreadCrumbs"/>.</summary>
public sealed class BreadcrumbItem
{
    public BreadcrumbItem(string text, string? href = null)
    {
        Text = text;
        Href = href;
    }

    /// <summary>Label shown for this segment.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>When null or whitespace, this segment is not a link (e.g. current page or a static label).</summary>
    public string? Href { get; set; }

    public bool IsClickable => !string.IsNullOrWhiteSpace(Href);
}

public partial class BreadCrumbs : ComponentBase
{
    /// <summary>Segments in order from root to leaf. Items without <see cref="BreadcrumbItem.Href"/> render as plain text.</summary>
    [Parameter]
    public IReadOnlyList<BreadcrumbItem>? Items { get; set; }

    /// <summary>Value for the wrapping <c>nav</c> <c>aria-label</c>.</summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Breadcrumb";

    /// <summary>Optional extra classes on the root <c>nav</c>.</summary>
    [Parameter]
    public string? CssClass { get; set; }
}
