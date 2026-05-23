using Client.Utils.UserPermissions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Components.Authorization;

public partial class CustomAutorizeView : ComponentBase
{
    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Parameter]
    public string? Role { get; set; }

    [Parameter]
    public string NotAuthorizedRoute { get; set; } = "/not-authorized";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool _isAuthorized;

    protected override async Task OnParametersSetAsync()
    {
        await AuthService.RefreshClientAuthStateAsync();

        var normalizedRole = Role?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedRole))
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _isAuthorized = authState.User.Identity?.IsAuthenticated == true;
        }
        else if (normalizedRole.Contains(',', StringComparison.Ordinal))
        {
            var roles = normalizedRole
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            _isAuthorized = await AuthService.IsInAnyRoleAsync(roles);
        }
        else
        {
            _isAuthorized = await AuthService.IsInRoleAsync(normalizedRole);
        }

        if (!_isAuthorized)
            Navigation.NavigateTo(NotAuthorizedRoute);
    }
}
