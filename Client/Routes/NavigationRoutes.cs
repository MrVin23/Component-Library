using System.Security.Claims;
using Client.Components.Navbars.SideNavBar;
using Microsoft.AspNetCore.Components.Routing;

namespace Client.Routes;

/// <summary>
/// Describes who is allowed to see/click a navigation item.
/// Think of this as simple "visibility rules" for menu links.
/// </summary>
public enum NavigationAccess
{
    /// <summary>Anyone can see it (including anonymous users).</summary>
    Public,
    /// <summary>Only users in either the User or Admin role can see it.</summary>
    UserOrAdmin,
    /// <summary>Only users in the Admin role can see it.</summary>
    Admin,
}

/// <summary>
/// Shared helper that answers: "Can this user access this nav item?"
/// Centralizing this keeps role logic in one place.
/// </summary>
public static class NavigationAccessHelper
{
    /// <summary>
    /// Returns true when the user is allowed to access an item with the provided access level.
    /// </summary>
    public static bool HasAccess(ClaimsPrincipal? user, NavigationAccess access)
    {
        // Public links are always visible, no login required.
        if (access == NavigationAccess.Public)
            return true;

        // Any non-public item requires an authenticated user.
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        // For authenticated users, map access type to required role checks.
        return access switch
        {
            NavigationAccess.UserOrAdmin => IsInAnyRole(user, "User", "Admin"),
            NavigationAccess.Admin => IsInAnyRole(user, "Admin"),
            _ => false,
        };
    }

    /// <summary>
    /// Checks whether a user has at least one role from the provided role list.
    /// Role checks are case-insensitive ("admin" == "Admin").
    /// </summary>
    private static bool IsInAnyRole(ClaimsPrincipal user, params string[] roles)
    {
        if (roles.Length == 0)
            return false;

        var allowedRoles = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return user.Claims.Any(c => c.Type == ClaimTypes.Role && allowedRoles.Contains(c.Value));
    }
}

/// <summary>
/// One navigation row configuration object.
/// Each item describes where it goes, what it looks like, and who can see it.
/// </summary>
public sealed record NavigationRouteItem(
    // Relative URL for the destination (example: "authorization-tests").
    string Href,
    // Text shown to the user in the sidebar.
    string Label,
    // Which icon to render next to the label.
    SideNavIconId Icon,
    // Access rule; defaults to Public if not specified.
    NavigationAccess Access = NavigationAccess.Public,
    // Blazor active-link match behavior.
    NavLinkMatch Match = NavLinkMatch.Prefix,
    // Optional tooltip/title text; null means use default behavior.
    string? Title = null);

/// <summary>
/// Central place for all sidebar navigation definitions.
/// Add/edit/remove nav items here instead of scattering them in UI components.
/// </summary>
public static class NavigationRoutes
{
    /// <summary>
    /// Top-level nav items always visible in the main sidebar section.
    /// </summary>
    public static readonly NavigationRouteItem[] MainNavItems =
    [
        new("", "Home", SideNavIconId.Home, Match: NavLinkMatch.All),
        new("usermanagement-dashboard", "User Management", SideNavIconId.UserManagement, NavigationAccess.Admin),
    ];

    /// <summary>
    /// Items shown inside the "Component library" dropdown section.
    /// </summary>
    public static readonly NavigationRouteItem[] ComponentLibraryNavItems =
    [
        new("componentlibrary-button", "Button", SideNavIconId.ComponentLibrary),
        new("componentlibrary-selects", "Selects", SideNavIconId.ComponentLibrary),
        new("componentlibrary-formcontainer", "Form container", SideNavIconId.ComponentLibrary),
        new("componentlibrary-alert", "Alert", SideNavIconId.ComponentLibrary),
        new("componentlibrary-snackbar", "Snackbar", SideNavIconId.ComponentLibrary),
        new("authorization-tests", "Authorization tests", SideNavIconId.ComponentLibrary, NavigationAccess.Admin),
    ];
}
