using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Client.Apis.UserPermissions;
using Client.Utils.UserPermissions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Dtos;
using Shared.Dtos.Users;

namespace Client.Pages;

public partial class Login
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Inject] private ApiAuth AuthApi { get; set; } = null!;
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IValidator<LoginRequest> LoginValidator { get; set; } = null!;
    [Inject] private IValidator<SignUpRequest> SignUpValidator { get; set; } = null!;

    private bool isSignUpMode;
    private LoginRequest loginRequest = new();
    private SignUpRequest signUpRequest = new();
    private bool isLoading;
    private string errorMessage = string.Empty;
    private string successMessage = string.Empty;
    private bool showPassword;
    private bool showConfirmPassword;
    private Dictionary<string, string[]>? validationErrors;

    /// <summary>Reused so <see cref="ValidationMessageStore.Clear"/> removes prior FluentValidation messages; a new store each submit leaves old stores attached to the <see cref="EditContext"/>.</summary>
    private EditContext? fluentValidationEditContext;
    private ValidationMessageStore? fluentValidationStore;

    /// <summary>Set when the optional &quot;already signed in&quot; probe fails (API down, CORS, wrong URL, TLS).</summary>
    private string? connectionHint;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var response = await AuthApi.GetCurrentUserAsync();
            if (response.StatusCode != HttpStatusCode.OK)
                return;

            var payload = await ReadApiResponseAsync<LoginResponse>(response);
            if (payload is { Success: true, Data: not null })
                Navigation.NavigateTo("/");
        }
        catch (HttpRequestException)
        {
            connectionHint =
                "Could not reach the API. Start the Server project, then confirm wwwroot/appsettings.json \"ApiBaseUrl\" matches that server (including http/https and port). For cross-origin calls, the server must list this site’s origin under CORS.";
        }
        catch (TaskCanceledException)
        {
            connectionHint = "The request to the API timed out. Check your network and that the server is running.";
        }
    }

    private async Task HandleLoginSubmit(EditContext editContext)
    {
        var result = await LoginValidator.ValidateAsync(loginRequest);
        ApplyFluentValidationResult(editContext, result);
        if (!result.IsValid)
            return;

        await HandleLoginAsync();
    }

    private async Task HandleSignUpSubmit(EditContext editContext)
    {
        var result = await SignUpValidator.ValidateAsync(signUpRequest);
        ApplyFluentValidationResult(editContext, result);
        if (!result.IsValid)
            return;

        await HandleSignUpAsync();
    }

    private void ApplyFluentValidationResult(EditContext editContext, ValidationResult result)
    {
        if (fluentValidationEditContext != editContext)
        {
            fluentValidationEditContext = editContext;
            fluentValidationStore = new ValidationMessageStore(editContext);
        }

        fluentValidationStore!.Clear();
        foreach (var error in result.Errors)
            fluentValidationStore.Add(editContext.Field(error.PropertyName), error.ErrorMessage);

        editContext.NotifyValidationStateChanged();
    }

    private async Task HandleLoginAsync()
    {
        ClearMessages();
        isLoading = true;
        try
        {
            var response = await AuthApi.LoginAsync(loginRequest);
            if (response.IsSuccessStatusCode)
            {
                var payload = await ReadApiResponseAsync<LoginResponse>(response);
                if (payload is { Success: true, Data: not null })
                {
                    await AuthService.RefreshClientAuthStateAsync();
                    Navigation.NavigateTo("/");
                    return;
                }

                errorMessage = payload?.Message ?? "Sign in failed.";
                return;
            }

            var err = await ReadApiErrorAsync(response);
            errorMessage = err?.Message ?? response.ReasonPhrase ?? "Sign in failed.";
        }
        catch (Exception ex)
        {
            errorMessage = $"Could not reach the server: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleSignUpAsync()
    {
        ClearMessages();
        isLoading = true;
        try
        {
            var response = await AuthApi.SignUpAsync(signUpRequest);

            if (response.IsSuccessStatusCode)
            {
                successMessage = "Account created. You can sign in now.";
                var username = signUpRequest.Username;
                signUpRequest = new SignUpRequest();
                loginRequest = new LoginRequest { Username = username };
                isSignUpMode = false;
                return;
            }

            var err = await ReadApiErrorAsync(response);
            if (err?.ValidationErrors is { Count: > 0 })
            {
                validationErrors = err.ValidationErrors;
                errorMessage = err.Message;
                return;
            }

            errorMessage = err?.Message ?? response.ReasonPhrase ?? "Sign up failed.";
        }
        catch (Exception ex)
        {
            errorMessage = $"Could not reach the server: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void SwitchToSignUp()
    {
        isSignUpMode = true;
        ClearMessages();
        signUpRequest = new SignUpRequest();
    }

    private void SwitchToSignIn()
    {
        isSignUpMode = false;
        ClearMessages();
        loginRequest = new LoginRequest();
    }

    private void ClearMessages()
    {
        errorMessage = string.Empty;
        successMessage = string.Empty;
        validationErrors = null;
    }

    private void TogglePasswordVisibility()
    {
        showPassword = !showPassword;
    }

    private void ToggleConfirmPasswordVisibility()
    {
        showConfirmPassword = !showConfirmPassword;
    }

    private string? GetFieldError(string fieldName)
    {
        if (validationErrors is null || !validationErrors.TryGetValue(fieldName, out var errors))
            return null;

        return errors.FirstOrDefault();
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
