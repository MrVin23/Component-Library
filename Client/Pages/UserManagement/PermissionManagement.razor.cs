using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Client.Apis.UserPermissions;
using Client.Components.Breadcrumbs;
using Client.Components.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Shared.Dtos;
using Shared.Dtos.UserPermissions;

namespace Client.Pages.UserManagement;

public partial class PermissionManagement : ComponentBase
{
    private static readonly IReadOnlyList<BreadcrumbItem> BreadcrumbItems =
    [
        new BreadcrumbItem("Home", "/"),
        new BreadcrumbItem("User management", "/usermanagement-dashboard"),
        new BreadcrumbItem("Permissions", href: null),
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Regex PermissionNameRegex = new("^[a-zA-Z0-9_\\-.:]+$", RegexOptions.Compiled);

    [Inject] private ApiPermissionsAndRoles PermissionsApi { get; set; } = null!;

    private Snackbar? feedbackSnackbar;

    private readonly List<PermissionResponse> permissions = new();
    private CreatePermissionRequest createRequest = NewCreateRequest();

    private EditContext? createEditContext;
    private ValidationMessageStore? createValidationStore;

    private bool isLoadingList = true;
    private bool isSaving;
    private int? isDeletingId;
    private int? editingPermissionId;
    private bool highlightPrimaryForm;
    private string bannerMessage = string.Empty;
    private bool bannerIsError;

    private string BannerCssClass =>
        bannerIsError ? "permission-management__banner permission-management__banner--error"
            : "permission-management__banner permission-management__banner--success";
    private string PrimaryFormTitle => editingPermissionId.HasValue ? "Edit permission" : "Create permission";

    protected override async Task OnInitializedAsync() => await ReloadPermissionsAsync();

    private static CreatePermissionRequest NewCreateRequest() => new()
    {
        Name = string.Empty,
        Description = string.Empty
    };

    private async Task HandleCreateResetAsync(MouseEventArgs _)
    {
        editingPermissionId = null;
        createRequest = NewCreateRequest();
        createValidationStore?.Clear();
        createEditContext?.NotifyValidationStateChanged();
        ClearBanner();
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleCreateSubmitAsync(EditContext editContext)
    {
        ClearBanner();
        createEditContext = editContext;
        createValidationStore = ValidatePermissionRequest(editContext, createRequest.Name, createRequest.Description, createValidationStore);
        if (editContext.GetValidationMessages().Any())
            return;

        createRequest.Name = createRequest.Name.Trim();
        createRequest.Description ??= string.Empty;

        isSaving = true;
        try
        {
            if (editingPermissionId.HasValue)
            {
                var updateRequest = new UpdatePermissionRequest
                {
                    Name = createRequest.Name,
                    Description = createRequest.Description
                };

                var updateResponse = await PermissionsApi.UpdatePermissionAsync(editingPermissionId.Value, updateRequest);
                if (updateResponse.IsSuccessStatusCode)
                {
                    var payload = await ReadApiResponseAsync<PermissionResponse>(updateResponse);
                    var permissionName = payload?.Data?.Name ?? updateRequest.Name;
                    feedbackSnackbar?.Show($"Permission '{permissionName}' updated.", AlertSeverity.Success, title: "Updated");
                    editingPermissionId = null;
                    createRequest = NewCreateRequest();
                    await ReloadPermissionsAsync();
                    return;
                }

                if (updateResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    SetBanner("That permission no longer exists.", success: false);
                    editingPermissionId = null;
                    createRequest = NewCreateRequest();
                    await ReloadPermissionsAsync();
                    return;
                }

                var updateErr = await ReadApiErrorAsync(updateResponse);
                SetBanner(updateErr?.Message ?? updateResponse.ReasonPhrase ?? "Could not update permission.", success: false);
                return;
            }

            var response = await PermissionsApi.CreatePermissionAsync(createRequest);
            if (response.IsSuccessStatusCode)
            {
                var payload = await ReadApiResponseAsync<PermissionResponse>(response);
                if (payload is { Success: true, Data: not null })
                {
                    feedbackSnackbar?.Show($"Permission '{payload.Data.Name}' created.", AlertSeverity.Success, title: "Created");
                    createRequest = NewCreateRequest();
                    await ReloadPermissionsAsync();
                    return;
                }

                SetBanner(payload?.Message ?? "Could not create permission.", success: false);
                return;
            }

            var err = await ReadApiErrorAsync(response);
            SetBanner(err?.Message ?? response.ReasonPhrase ?? "Could not create permission.", success: false);
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

    private void StartEdit(PermissionResponse permission)
    {
        ClearBanner();
        editingPermissionId = permission.Id;
        createRequest = new CreatePermissionRequest
        {
            Name = permission.Name,
            Description = permission.Description
        };
        createValidationStore?.Clear();
        createEditContext?.NotifyValidationStateChanged();
        _ = HighlightPrimaryFormAsync();
    }

    private Task StartEditFromRowAsync(PermissionResponse permission)
    {
        StartEdit(permission);
        return Task.CompletedTask;
    }

    private async Task HighlightPrimaryFormAsync()
    {
        highlightPrimaryForm = true;
        await InvokeAsync(StateHasChanged);
        await Task.Delay(1100);
        highlightPrimaryForm = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task DeletePermissionAsync(int id)
    {
        ClearBanner();
        isDeletingId = id;
        var removedName = permissions.FirstOrDefault(p => p.Id == id)?.Name;
        try
        {
            var response = await PermissionsApi.DeletePermissionAsync(id);
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                feedbackSnackbar?.Show(
                    string.IsNullOrWhiteSpace(removedName) ? "Permission deleted." : $"Permission '{removedName}' deleted.",
                    AlertSeverity.Success,
                    title: "Deleted");

                if (editingPermissionId == id)
                {
                    editingPermissionId = null;
                    createRequest = NewCreateRequest();
                }

                await ReloadPermissionsAsync();
                return;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                SetBanner("That permission was already removed.", success: false);
                await ReloadPermissionsAsync();
                return;
            }

            var err = await ReadApiErrorAsync(response);
            SetBanner(err?.Message ?? response.ReasonPhrase ?? "Could not delete permission.", success: false);
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

    private Task DeletePermissionFromRowAsync(PermissionResponse permission) => DeletePermissionAsync(permission.Id);

    private async Task ReloadPermissionsAsync()
    {
        isLoadingList = true;
        try
        {
            var response = await PermissionsApi.GetPermissionsAsync(pageNumber: 1, pageSize: 500);
            if (!response.IsSuccessStatusCode)
            {
                permissions.Clear();
                var err = await ReadApiErrorAsync(response);
                SetBanner(err?.Message ?? "Could not load permissions.", success: false);
                return;
            }

            var payload = await ReadPaginatedApiResponseAsync<PermissionResponse>(response);
            if (payload is { Success: true, Data: not null })
            {
                permissions.Clear();
                permissions.AddRange(payload.Data.OrderByDescending(p => p.CreatedAt));
            }
            else
            {
                permissions.Clear();
                SetBanner(payload?.Message ?? "Could not load permissions.", success: false);
            }
        }
        catch (Exception ex)
        {
            permissions.Clear();
            SetBanner($"Could not load permissions: {ex.Message}", success: false);
        }
        finally
        {
            isLoadingList = false;
        }
    }

    private static ValidationMessageStore ValidatePermissionRequest(
        EditContext editContext,
        string name,
        string description,
        ValidationMessageStore? store)
    {
        store ??= new ValidationMessageStore(editContext);
        store.Clear();

        var trimmedName = name?.Trim() ?? string.Empty;
        var normalizedDescription = description ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedName))
            store.Add(editContext.Field("Name"), "Permission name is required");
        else if (trimmedName.Length > 100)
            store.Add(editContext.Field("Name"), "Permission name must not exceed 100 characters");
        else if (!PermissionNameRegex.IsMatch(trimmedName))
            store.Add(editContext.Field("Name"), "Permission name can only contain letters, numbers, underscores, hyphens, dots, and colons");

        if (normalizedDescription.Length > 500)
            store.Add(editContext.Field("Description"), "Description must not exceed 500 characters");

        editContext.NotifyValidationStateChanged();
        return store;
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

    private static async Task<PaginatedApiResponse<T>?> ReadPaginatedApiResponseAsync<T>(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<PaginatedApiResponse<T>>(JsonOptions);
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
