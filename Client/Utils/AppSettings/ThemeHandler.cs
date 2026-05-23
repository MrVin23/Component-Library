using Client.Interfaces.Authorisation;
using Microsoft.JSInterop;
using Shared.Dtos.Users;

namespace Client.Utils.AppSettings;

/// <summary>
/// Applies <c>data-theme</c> from encrypted session (<see cref="ClientSession"/>) and notifies subscribers.
/// </summary>
public sealed class ThemeHandler
{
    private readonly ISecureStorageService _secureStorage;
    private readonly IJSRuntime _js;

    public ThemeHandler(ISecureStorageService secureStorage, IJSRuntime js)
    {
        _secureStorage = secureStorage;
        _js = js;
    }

    /// <summary>Current dark mode preference (matches session when authenticated).</summary>
    public bool IsDarkMode { get; private set; } = true;

    public event Action? Changed;

    public async Task ApplyFromStoredSessionAsync()
    {
        var session = await ClientSessionStorage.ReadAsync(_secureStorage);
        var dark = ResolveDarkMode(session);
        IsDarkMode = dark;
        await ApplyDomAsync(dark);
        RaiseChanged();
    }

    /// <summary>Optimistic update: session + DOM. Callers (navbar, app settings) may follow with an API save.</summary>
    public async Task SetThemeLocalAsync(bool dark)
    {
        var session = await ClientSessionStorage.ReadAsync(_secureStorage);
        if (session?.User == null)
        {
            IsDarkMode = dark;
            await ApplyDomAsync(dark);
            RaiseChanged();
            return;
        }

        session.Settings ??= ClientSessionStorage.DefaultSettings(session.User.Id);
        session.Settings.DarkMode = dark;
        session.Settings.UserId = session.User.Id;
        await _secureStorage.SetAsync(ClientSessionStorage.SessionKey, session);

        IsDarkMode = dark;
        await ApplyDomAsync(dark);
        RaiseChanged();
    }

    /// <summary>After settings API load/save: keep session and DOM in sync with server.</summary>
    public async Task SyncSettingsAsync(UserSettingsResponse settings)
    {
        var session = await ClientSessionStorage.ReadAsync(_secureStorage);
        if (session?.User == null)
        {
            IsDarkMode = settings.DarkMode;
            await ApplyDomAsync(settings.DarkMode);
            RaiseChanged();
            return;
        }

        session.Settings = settings;
        await _secureStorage.SetAsync(ClientSessionStorage.SessionKey, session);

        IsDarkMode = settings.DarkMode;
        await ApplyDomAsync(settings.DarkMode);
        RaiseChanged();
    }

    private static bool ResolveDarkMode(ClientSession? session)
    {
        if (session?.Settings != null)
            return session.Settings.DarkMode;
        return true;
    }

    private async Task ApplyDomAsync(bool dark)
    {
        try
        {
            await _js.InvokeVoidAsync("themeInterop.setTheme", dark ? "dark" : "light");
        }
        catch (JSException)
        {
            // Script may not be available yet
        }
    }

    private void RaiseChanged() => Changed?.Invoke();
}
