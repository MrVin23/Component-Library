using Microsoft.AspNetCore.Components.Routing;

namespace Client.Components.Navbars.SideNavBar;

public enum SideNavIconId
{
    Home,
    UserManagement,
    Counter,
    Weather,
    ComponentLibrary,
    More,
    NotFound,
    SignIn,
}

/// <summary>One main nav row: set Href, Label, Icon; optional Title for tooltip.</summary>
public sealed record SideNavLinkItem(
    string Href,
    string Label,
    SideNavIconId Icon,
    NavLinkMatch Match = NavLinkMatch.Prefix,
    string? Title = null)
{
    public string Tooltip => Title ?? Label;
}
