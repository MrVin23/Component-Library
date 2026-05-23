using Microsoft.AspNetCore.Components;

namespace Client.Components.Navbars.NavBarDropdown;

public partial class NavBarDropdown
{
    private readonly string _panelId = $"nav-dropdown-{Guid.NewGuid():N}";
    private bool _open;

    [CascadingParameter(Name = "SideNavCollapsed")]
    public bool SideNavCollapsed { get; set; }

    [Parameter]
    public string Label { get; set; } = "";

    [Parameter]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string RootClass
    {
        get
        {
            var s = "nav-bar-dropdown";
            if (SideNavCollapsed)
            {
                s += " nav-bar-dropdown--sidebar-collapsed";
            }

            if (_open)
            {
                s += " nav-bar-dropdown--open";
            }

            return s;
        }
    }

    private string PanelClass =>
        _open ? "nav-bar-dropdown__panel nav-bar-dropdown__panel--visible" : "nav-bar-dropdown__panel";

    private void Toggle() => _open = !_open;
}
