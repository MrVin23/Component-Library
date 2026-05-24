using Client.Interfaces.Authorisation;
using Microsoft.JSInterop;
using Shared.Dtos.Users;

namespace Client.Utils.AppSettings;

// security risk: DEMO ONLY — revert localStorage/offline theme paths before production (see securityNotes.md).
/// <summary>
/// Applies <c>data-theme</c> from encrypted session (<see cref="ClientSession"/>) or browser localStorage (offline).
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

    /// <summary>Current dark mode preference.</summary>
    public bool IsDarkMode { get; private set; }

    public event Action? Changed;

    public async Task ApplyFromStoredSessionAsync()
    {
        var session = await ClientSessionStorage.ReadAsync(_secureStorage);
        var dark = session?.Settings != null
            ? session.Settings.DarkMode
            // security risk: DEMO ONLY — use session/server default only in production (remove ReadDarkModeFromBrowserAsync fallback).
            : await ReadDarkModeFromBrowserAsync();

        IsDarkMode = dark;
        await ApplyDomAsync(dark);
        RaiseChanged();
    }

    /// <summary>Optimistic update: localStorage + optional session. API save is caller responsibility.</summary>
    public async Task SetThemeLocalAsync(bool dark)
    {
        var session = await ClientSessionStorage.ReadAsync(_secureStorage);
        if (session?.User != null)
        {
            session.Settings ??= ClientSessionStorage.DefaultSettings(session.User.Id);
            session.Settings.DarkMode = dark;
            session.Settings.UserId = session.User.Id;
            await _secureStorage.SetAsync(ClientSessionStorage.SessionKey, session);
        }

        IsDarkMode = dark;
        // security risk: DEMO ONLY — setTheme writes to localStorage via theme.js; restrict to authenticated session in production if needed.
        await ApplyDomAsync(dark);
        RaiseChanged();
    }

    /// <summary>After settings API load/save: keep session and DOM in sync with server.</summary>
    public async Task SyncSettingsAsync(UserSettingsResponse settings)
    {
        var session = await ClientSessionStorage.ReadAsync(_secureStorage);
        if (session?.User == null)
        {
            // security risk: DEMO ONLY — do not apply settings for anonymous users in production.
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

    // security risk: DEMO ONLY — remove before production; theme should come from server session only.
    private async Task<bool> ReadDarkModeFromBrowserAsync()
    {
        try
        {
            return await _js.InvokeAsync<bool>("themeInterop.isDarkMode");
        }
        catch (JSException)
        {
            return false;
        }
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
