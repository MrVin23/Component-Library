using Microsoft.AspNetCore.Components;

namespace Client.Routes;

// security risk: DEMO ONLY — delete this entire file before production.
// Allows anonymous/offline access to component-library story routes without auth or antiforgery.
// See securityNotes.md. Search codebase for "security risk" to find all demo bypasses.

/// <summary>
/// Public component-library story routes that work without API auth or antiforgery.
/// </summary>
public static class ComponentLibraryRoutes
{
    public const string DefaultRoute = "/componentlibrary-button";

    private static readonly string[] PublicRelativePaths = NavigationRoutes.ComponentLibraryNavItems
        .Where(item => item.Access == NavigationAccess.Public)
        .Select(item => item.Href.Trim().TrimStart('/'))
        .Where(href => href.Length > 0)
        .ToArray();

    public static bool IsPublicStoryRoute(NavigationManager navigation)
    {
        var path = navigation.ToBaseRelativePath(navigation.Uri)
            .Split('?', '#')[0]
            .Trim()
            .TrimEnd('/');

        if (path.Length == 0)
            return false;

        return PublicRelativePaths.Any(segment =>
            path.Equals(segment, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(segment + "/", StringComparison.OrdinalIgnoreCase));
    }
}
