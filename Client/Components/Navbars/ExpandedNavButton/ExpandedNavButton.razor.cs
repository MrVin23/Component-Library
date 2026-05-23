using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Client.Components.Navbars;

public partial class ExpandedNavButton : IDisposable
{
    [Inject]
    public NavigationManager NavManager { get; set; } = default!;

    [Parameter]
    public string Href { get; set; } = "";

    [Parameter]
    public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;

    [Parameter]
    public string Label { get; set; } = "";

    [Parameter]
    public RenderFragment? Icon { get; set; }

    private string ButtonClass =>
        IsActive ? "expanded-nav-button expanded-nav-button--active" : "expanded-nav-button";

    private bool IsActive
    {
        get
        {
            var currentPath = NormalizeRoutePath(NavManager.ToBaseRelativePath(NavManager.Uri));
            var targetPath = NormalizeRoutePath(Href);

            if (Match == NavLinkMatch.All)
            {
                return string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase);
            }

            if (string.IsNullOrEmpty(targetPath))
            {
                return string.IsNullOrEmpty(currentPath);
            }

            return currentPath.Equals(targetPath, StringComparison.OrdinalIgnoreCase)
                || currentPath.StartsWith($"{targetPath}/", StringComparison.OrdinalIgnoreCase);
        }
    }

    protected override void OnInitialized()
    {
        NavManager.LocationChanged += OnLocationChanged;
    }

    private void Navigate()
    {
        NavManager.NavigateTo(Href);
    }

    private static string NormalizeRoutePath(string route)
    {
        var queryless = route.Split('?', '#')[0];
        return queryless.Trim('/');
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        NavManager.LocationChanged -= OnLocationChanged;
    }
}
