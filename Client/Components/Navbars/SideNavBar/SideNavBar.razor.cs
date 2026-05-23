using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Authorization;
using Client.Routes;

namespace Client.Components.Navbars.SideNavBar;

public partial class SideNavBar : IDisposable
{
    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Parameter]
    public bool Collapsed { get; set; }

    private IReadOnlyList<SideNavLinkItem> VisibleMainNavItems { get; set; } = [];
    private IReadOnlyList<SideNavLinkItem> VisibleComponentLibraryNavItems { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        AuthStateProvider.AuthenticationStateChanged += HandleAuthenticationStateChanged;
        await RefreshVisibleNavItemsAsync();
    }

    private async void HandleAuthenticationStateChanged(Task<AuthenticationState> _)
    {
        await RefreshVisibleNavItemsAsync();
    }

    private async Task RefreshVisibleNavItemsAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        VisibleMainNavItems = NavigationRoutes.MainNavItems
            .Where(item => NavigationAccessHelper.HasAccess(user, item.Access))
            .Select(ToSideNavLinkItem)
            .ToArray();

        VisibleComponentLibraryNavItems = NavigationRoutes.ComponentLibraryNavItems
            .Where(item => NavigationAccessHelper.HasAccess(user, item.Access))
            .Select(ToSideNavLinkItem)
            .ToArray();

        await InvokeAsync(StateHasChanged);
    }

    private static SideNavLinkItem ToSideNavLinkItem(NavigationRouteItem item) =>
        new(item.Href, item.Label, item.Icon, item.Match, item.Title);

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= HandleAuthenticationStateChanged;
    }
}
