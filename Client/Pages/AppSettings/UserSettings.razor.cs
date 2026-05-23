using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Client.Apis.AppSettings;
using Client.Components.Validation;
using Client.Utils.AppSettings;
using Microsoft.AspNetCore.Components;
using Shared.Dtos;
using Shared.Dtos.Users;

namespace Client.Pages.AppSettings;

public partial class UserSettings : ComponentBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Inject]
    private ApiUserSettings SettingsApi { get; set; } = null!;

    [Inject]
    private ThemeHandler ThemeHandler { get; set; } = null!;

    private Snackbar? _snackbar;

    private bool darkMode = true;
    private bool isLoading = true;
    private bool isSaving;
    private string loadError = string.Empty;

    protected override async Task OnInitializedAsync() => await LoadSettingsAsync();

    private async Task LoadSettingsAsync()
    {
        isLoading = true;
        loadError = string.Empty;

        try
        {
            var response = await SettingsApi.GetMineAsync();
            if (response.IsSuccessStatusCode)
            {
                var payload = await ReadApiResponseAsync<UserSettingsResponse>(response);
                if (payload is { Success: true, Data: not null })
                {
                    darkMode = payload.Data.DarkMode;
                    await ThemeHandler.SyncSettingsAsync(payload.Data);
                    return;
                }

                loadError = payload?.Message ?? "Could not read your settings.";
                return;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                loadError = "You need to sign in to manage app settings.";
                return;
            }

            var err = await ReadApiErrorAsync(response);
            loadError = err?.Message ?? response.ReasonPhrase ?? "Could not load settings.";
        }
        catch (Exception ex)
        {
            loadError = $"Request failed: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task OnDarkModeToggleAsync(bool nextDarkMode)
    {
        if (isSaving)
            return;

        var previous = darkMode;
        darkMode = nextDarkMode;
        isSaving = true;

        try
        {
            await ThemeHandler.SetThemeLocalAsync(nextDarkMode);

            var response = await SettingsApi.UpdateMineAsync(new UpdateUserSettingsRequest { DarkMode = nextDarkMode });
            if (response.IsSuccessStatusCode)
            {
                var payload = await ReadApiResponseAsync<UserSettingsResponse>(response);
                if (payload?.Data != null)
                {
                    darkMode = payload.Data.DarkMode;
                    await ThemeHandler.SyncSettingsAsync(payload.Data);
                }

                _snackbar?.Show("Appearance saved.", AlertSeverity.Success, title: "Saved");
                return;
            }

            darkMode = previous;
            await ThemeHandler.SetThemeLocalAsync(previous);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _snackbar?.Show("Your session may have expired. Sign in again.", AlertSeverity.Warning);
                return;
            }

            var err = await ReadApiErrorAsync(response);
            _snackbar?.Show(err?.Message ?? "Could not save settings.", AlertSeverity.Error);
        }
        catch (Exception ex)
        {
            darkMode = previous;
            await ThemeHandler.SetThemeLocalAsync(previous);
            _snackbar?.Show($"Request failed: {ex.Message}", AlertSeverity.Error);
        }
        finally
        {
            isSaving = false;
        }
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

    private static async Task<ApiError?> ReadApiErrorAsync(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiError>(JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
