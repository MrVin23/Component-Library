using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Client.Apis.UserPermissions;
using Client.Components.Breadcrumbs;
using Client.Components.Validation;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Shared.Dtos;
using Shared.Dtos.UserPermissions;

namespace Client.Pages.UserManagement;

public partial class SignUpKeyManagement : ComponentBase
{
    private static readonly IReadOnlyList<BreadcrumbItem> BreadcrumbItems =
    [
        new BreadcrumbItem("Home", "/"),
        new BreadcrumbItem("User management", "/usermanagement-dashboard"),
        new BreadcrumbItem("Sign-up keys", href: null),
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Inject] private ApiSignUpKey SignUpKeyApi { get; set; } = null!;
    [Inject] private IValidator<CreateSignUpKeyRequest> CreateValidator { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;

    private Snackbar? feedbackSnackbar;

    private readonly List<SignUpKeyResponse> keys = new();
    private CreateSignUpKeyRequest createRequest = NewDefaultCreateRequest();
    private EditContext? fluentValidationEditContext;
    private ValidationMessageStore? fluentValidationStore;

    private bool isLoadingList = true;
    private bool isSaving;
    private int? isDeletingId;
    private string bannerMessage = string.Empty;
    private bool bannerIsError;

    private string BannerCssClass =>
        bannerIsError ? "signup-key-management__banner signup-key-management__banner--error"
            : "signup-key-management__banner signup-key-management__banner--success";

    protected override async Task OnInitializedAsync() => await ReloadKeysAsync();

    private async Task CopyKeyToClipboardAsync(string key)
    {
        try
        {
            await Js.InvokeVoidAsync("themeInterop.copyText", key);
            feedbackSnackbar?.Show("Key copied to clipboard.", AlertSeverity.Success, title: "Copied");
        }
        catch (JSException)
        {
            feedbackSnackbar?.Show("Clipboard is not available in this browser or context.", AlertSeverity.Warning, title: "Copy failed");
        }
    }

    private static CreateSignUpKeyRequest NewDefaultCreateRequest() =>
        new()
        {
            Key = null,
            ExpiresAt = DateTime.UtcNow.AddHours(2)
        };

    private async Task HandleCreateResetAsync(MouseEventArgs _)
    {
        createRequest = NewDefaultCreateRequest();
        ClearBanner();
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleCreateSubmitAsync(EditContext editContext)
    {
        ClearBanner();
        if (string.IsNullOrWhiteSpace(createRequest.Key))
            createRequest.Key = null;

        var result = await CreateValidator.ValidateAsync(createRequest);
        ApplyFluentValidationResult(editContext, result);
        if (!result.IsValid)
            return;

        isSaving = true;
        try
        {
            var response = await SignUpKeyApi.CreateSignUpKeyAsync(createRequest);
            if (response.IsSuccessStatusCode)
            {
                var payload = await ReadApiResponseAsync<SignUpKeyResponse>(response);
                if (payload is { Success: true, Data: not null })
                {
                    var key = payload.Data.Key;
                    var snackMessage = $"Copy this key for the sign-up screen:{Environment.NewLine}{Environment.NewLine}{key}";
                    feedbackSnackbar?.Show(snackMessage, AlertSeverity.Success, title: "Sign-up key created");
                    createRequest = NewDefaultCreateRequest();
                    await ReloadKeysAsync();
                    return;
                }

                SetBanner(payload?.Message ?? "Could not create sign-up key.", success: false);
                return;
            }

            var err = await ReadApiErrorAsync(response);
            if (err?.ValidationErrors is { Count: > 0 })
            {
                ApplyServerValidation(editContext, err.ValidationErrors);
                SetBanner(err.Message, success: false);
                return;
            }

            SetBanner(err?.Message ?? response.ReasonPhrase ?? "Could not create sign-up key.", success: false);
        }
        catch (Exception ex)
        {
            SetBanner($"Request failed: {ex.Message}", success: false);
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DeleteKeyAsync(int id)
    {
        ClearBanner();
        var removedKey = keys.FirstOrDefault(k => k.Id == id)?.Key;
        isDeletingId = id;
        try
        {
            var response = await SignUpKeyApi.DeleteSignUpKeyAsync(id);
            if (response.IsSuccessStatusCode)
            {
                var snackMessage = string.IsNullOrEmpty(removedKey)
                    ? "The sign-up key was removed."
                    : $"This key is no longer valid for registration:{Environment.NewLine}{Environment.NewLine}{removedKey}";
                feedbackSnackbar?.Show(snackMessage, AlertSeverity.Success, title: "Sign-up key deleted");
                await ReloadKeysAsync();
                return;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                SetBanner("That key was already removed.", success: false);
                await ReloadKeysAsync();
                return;
            }

            var err = await ReadApiErrorAsync(response);
            SetBanner(err?.Message ?? response.ReasonPhrase ?? "Delete failed.", success: false);
        }
        catch (Exception ex)
        {
            SetBanner($"Request failed: {ex.Message}", success: false);
        }
        finally
        {
            isDeletingId = null;
        }
    }

    private Task DeleteKeyFromRowAsync(SignUpKeyResponse key) => DeleteKeyAsync(key.Id);

    private async Task ReloadKeysAsync()
    {
        isLoadingList = true;
        try
        {
            var response = await SignUpKeyApi.GetAllSignUpKeysAsync();
            if (!response.IsSuccessStatusCode)
            {
                var err = await ReadApiErrorAsync(response);
                SetBanner(err?.Message ?? "Could not load sign-up keys.", success: false);
                keys.Clear();
                return;
            }

            var payload = await ReadApiResponseAsync<IEnumerable<SignUpKeyResponse>>(response);
            if (payload is { Success: true, Data: not null })
            {
                keys.Clear();
                keys.AddRange(payload.Data.OrderByDescending(k => k.CreatedAt));
                if (string.IsNullOrEmpty(bannerMessage))
                    ClearBanner();
            }
            else
            {
                keys.Clear();
                SetBanner(payload?.Message ?? "Could not load sign-up keys.", success: false);
            }
        }
        catch (Exception ex)
        {
            keys.Clear();
            SetBanner($"Could not load keys: {ex.Message}", success: false);
        }
        finally
        {
            isLoadingList = false;
        }
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

    private void ApplyServerValidation(EditContext editContext, Dictionary<string, string[]> errors)
    {
        if (fluentValidationEditContext != editContext)
        {
            fluentValidationEditContext = editContext;
            fluentValidationStore = new ValidationMessageStore(editContext);
        }

        fluentValidationStore!.Clear();
        foreach (var (property, messages) in errors)
        {
            foreach (var message in messages)
                fluentValidationStore.Add(editContext.Field(property), message);
        }

        editContext.NotifyValidationStateChanged();
    }

    private void SetBanner(string message, bool success)
    {
        bannerMessage = message;
        bannerIsError = !success;
    }

    private void ClearBanner()
    {
        bannerMessage = string.Empty;
        bannerIsError = false;
    }

    private static string FormatDate(DateTime value) =>
        value.ToString("g", System.Globalization.CultureInfo.CurrentCulture);

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
