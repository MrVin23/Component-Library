using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Client.Components.Navbars.NavBarButton;

public partial class NavBarButton
{
    [CascadingParameter(Name = "SideNavCollapsed")]
    public bool SideNavCollapsed { get; set; }

    /// <summary>Route path (e.g. "counter", "" for home).</summary>
    [Parameter]
    public string Href { get; set; } = "";

    [Parameter]
    public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;

    /// <summary>Visible label when the sidebar is expanded.</summary>
    [Parameter]
    public string Label { get; set; } = "";

    /// <summary>Tooltip when the sidebar shows icon-only.</summary>
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }
}
