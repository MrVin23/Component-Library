using System.Net.Http.Json;
using System.Text.Json;
using Client.Apis.AppSettings;
using Client.Interfaces.Authorisation;
using Client.Utils.AppSettings;
using Client.Utils.UserPermissions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Shared.Dtos;
using Shared.Dtos.Users;

namespace Client.Components.Navbars.TopNavbar;

public partial class TopNavBar : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISecureStorageService SecureStorage { get; set; } = default!;

    [Inject]
    private ThemeHandler ThemeHandler { get; set; } = default!;

    [Inject]
    private ApiUserSettings SettingsApi { get; set; } = default!;

    [Parameter]
    public bool SidebarCollapsed { get; set; }

    [Parameter]
    public EventCallback OnSidebarToggle { get; set; }

    private bool _isDarkMode = true;
    private bool _themeSaving;
    private bool _accountMenuOpen;
    private bool _isSignedIn;
    private string _displayName = "Account";

    private async Task OnDarkModeChangedAsync(bool value)
    {
        if (_themeSaving)
            return;

        var previousDark = ThemeHandler.IsDarkMode;
        await ThemeHandler.SetThemeLocalAsync(value);
        _isDarkMode = ThemeHandler.IsDarkMode;

        _themeSaving = true;
        try
        {
            var response = await SettingsApi.UpdateMineAsync(new UpdateUserSettingsRequest { DarkMode = value });
            if (response.IsSuccessStatusCode)
            {
                var payload = await ReadApiResponseAsync<UserSettingsResponse>(response);
                if (payload?.Data != null)
                    await ThemeHandler.SyncSettingsAsync(payload.Data);
                return;
            }

            await ThemeHandler.SetThemeLocalAsync(previousDark);
            _isDarkMode = ThemeHandler.IsDarkMode;
        }
        catch
        {
            await ThemeHandler.SetThemeLocalAsync(previousDark);
            _isDarkMode = ThemeHandler.IsDarkMode;
        }
        finally
        {
            _themeSaving = false;
        }
    }

    private async Task NavigateToAccountAsync()
    {
        _accountMenuOpen = false;
        NavigationManager.NavigateTo("/account");
        await Task.CompletedTask;
    }

    private async Task NavigateToAppSettingsAsync()
    {
        _accountMenuOpen = false;
        NavigationManager.NavigateTo("/app-settings");
        await Task.CompletedTask;
    }
    private void ToggleAccountDropdown() => _accountMenuOpen = !_accountMenuOpen;
    private async Task OnAccountTriggerBlur(FocusEventArgs _)
    {
        // Delay allows menu-item click to run before closing.
        await Task.Delay(120);
        _accountMenuOpen = false;
        StateHasChanged();
    }

    private async Task LogoutAsync()
    {
        _accountMenuOpen = false;

        await AuthService.LogoutAsync();
        await AuthService.RefreshClientAuthStateAsync();
        await RefreshUserStateAsync();

        NavigationManager.NavigateTo("login", forceLoad: false);
    }

    protected override async Task OnInitializedAsync()
    {
        ThemeHandler.Changed += OnThemeHandlerChanged;
        _isDarkMode = ThemeHandler.IsDarkMode;
        await RefreshUserStateAsync();
    }

    private void OnThemeHandlerChanged()
    {
        _isDarkMode = ThemeHandler.IsDarkMode;
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose() => ThemeHandler.Changed -= OnThemeHandlerChanged;

    private async Task RefreshUserStateAsync()
    {
        var session = await ClientSessionStorage.ReadAsync(SecureStorage);
        var user = session?.User;
        _isSignedIn = user != null && !string.IsNullOrWhiteSpace(user.Username);

        if (!_isSignedIn)
        {
            _displayName = "Account";
            return;
        }

        var fullName = $"{user!.FirstName} {user.LastName}".Trim();
        _displayName = string.IsNullOrWhiteSpace(fullName)
            ? user.Username
            : fullName;
    }

    private static async Task<ApiResponse<T>?> ReadApiResponseAsync<T>(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        }
        catch
        {
            return null;
        }
    }

}
