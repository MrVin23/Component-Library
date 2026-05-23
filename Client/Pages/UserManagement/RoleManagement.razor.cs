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

public partial class RoleManagement : ComponentBase
{
    private static readonly IReadOnlyList<BreadcrumbItem> BreadcrumbItems =
    [
        new BreadcrumbItem("Home", "/"),
        new BreadcrumbItem("User management", "/usermanagement-dashboard"),
        new BreadcrumbItem("Roles", href: null),
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Regex RoleNameRegex = new("^[a-zA-Z0-9_\\-\\s]+$", RegexOptions.Compiled);

    [Inject] private ApiPermissionsAndRoles PermissionsApi { get; set; } = null!;

    private Snackbar? feedbackSnackbar;

    private readonly List<RoleWithPermissionsResponse> roles = new();
    private CreateRoleRequest createRequest = NewCreateRequest();

    private EditContext? createEditContext;
    private ValidationMessageStore? createValidationStore;

    private bool isLoadingList = true;
    private bool isSaving;
    private int? isDeletingId;
    private int? editingRoleId;
    private bool highlightPrimaryForm;
    private string bannerMessage = string.Empty;
    private bool bannerIsError;

    private string BannerCssClass =>
        bannerIsError ? "role-management__banner role-management__banner--error"
            : "role-management__banner role-management__banner--success";

    private string PrimaryFormTitle => editingRoleId.HasValue ? "Edit role" : "Create role";

    protected override async Task OnInitializedAsync() => await ReloadRolesAsync();

    private static CreateRoleRequest NewCreateRequest() => new()
    {
        Name = string.Empty,
        Description = string.Empty
    };

    private async Task HandleCreateResetAsync(MouseEventArgs _)
    {
        editingRoleId = null;
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
        createValidationStore = ValidateRoleRequest(editContext, createRequest.Name, createRequest.Description, createValidationStore);
        if (editContext.GetValidationMessages().Any())
            return;

        createRequest.Name = createRequest.Name.Trim();
        createRequest.Description ??= string.Empty;

        isSaving = true;
        try
        {
            if (editingRoleId.HasValue)
            {
                var updateRequest = new UpdateRoleRequest
                {
                    Name = createRequest.Name,
                    Description = createRequest.Description
                };

                var updateResponse = await PermissionsApi.UpdateRoleAsync(editingRoleId.Value, updateRequest);
                if (updateResponse.IsSuccessStatusCode)
                {
                    var payload = await ReadApiResponseAsync<RoleWithPermissionsResponse>(updateResponse);
                    var roleName = payload?.Data?.Name ?? updateRequest.Name;
                    feedbackSnackbar?.Show($"Role '{roleName}' updated.", AlertSeverity.Success, title: "Updated");
                    editingRoleId = null;
                    createRequest = NewCreateRequest();
                    await ReloadRolesAsync();
                    return;
                }

                if (updateResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    SetBanner("That role no longer exists.", success: false);
                    editingRoleId = null;
                    createRequest = NewCreateRequest();
                    await ReloadRolesAsync();
                    return;
                }

                var updateErr = await ReadApiErrorAsync(updateResponse);
                SetBanner(updateErr?.Message ?? updateResponse.ReasonPhrase ?? "Could not update role.", success: false);
                return;
            }

            var response = await PermissionsApi.CreateRoleAsync(createRequest);
            if (response.IsSuccessStatusCode)
            {
                var payload = await ReadApiResponseAsync<RoleWithPermissionsResponse>(response);
                if (payload is { Success: true, Data: not null })
                {
                    feedbackSnackbar?.Show($"Role '{payload.Data.Name}' created.", AlertSeverity.Success, title: "Created");
                    createRequest = NewCreateRequest();
                    await ReloadRolesAsync();
                    return;
                }

                SetBanner(payload?.Message ?? "Could not create role.", success: false);
                return;
            }

            var err = await ReadApiErrorAsync(response);
            SetBanner(err?.Message ?? response.ReasonPhrase ?? "Could not create role.", success: false);
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

    private void StartEdit(RoleWithPermissionsResponse role)
    {
        ClearBanner();
        editingRoleId = role.Id;
        createRequest = new CreateRoleRequest
        {
            Name = role.Name,
            Description = role.Description
        };
        createValidationStore?.Clear();
        createEditContext?.NotifyValidationStateChanged();
        _ = HighlightPrimaryFormAsync();
    }

    private Task StartEditFromRowAsync(RoleWithPermissionsResponse role)
    {
        StartEdit(role);
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

    private async Task DeleteRoleAsync(int id)
    {
        ClearBanner();
        isDeletingId = id;
        var removedName = roles.FirstOrDefault(r => r.Id == id)?.Name;
        try
        {
            var response = await PermissionsApi.DeleteRoleAsync(id);
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                feedbackSnackbar?.Show(
                    string.IsNullOrWhiteSpace(removedName) ? "Role deleted." : $"Role '{removedName}' deleted.",
                    AlertSeverity.Success,
                    title: "Deleted");

                if (editingRoleId == id)
                {
                    editingRoleId = null;
                    createRequest = NewCreateRequest();
                }

                await ReloadRolesAsync();
                return;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                SetBanner("That role was already removed.", success: false);
                await ReloadRolesAsync();
                return;
            }

            var err = await ReadApiErrorAsync(response);
            SetBanner(err?.Message ?? response.ReasonPhrase ?? "Could not delete role.", success: false);
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

    private Task DeleteRoleFromRowAsync(RoleWithPermissionsResponse role) => DeleteRoleAsync(role.Id);

    private async Task ReloadRolesAsync()
    {
        isLoadingList = true;
        try
        {
            var response = await PermissionsApi.GetRolesAsync(pageNumber: 1, pageSize: 500);
            if (!response.IsSuccessStatusCode)
            {
                roles.Clear();
                var err = await ReadApiErrorAsync(response);
                SetBanner(err?.Message ?? "Could not load roles.", success: false);
                return;
            }

            var payload = await ReadPaginatedApiResponseAsync<RoleWithPermissionsResponse>(response);
            if (payload is { Success: true, Data: not null })
            {
                roles.Clear();
                roles.AddRange(payload.Data.OrderByDescending(r => r.CreatedAt));
            }
            else
            {
                roles.Clear();
                SetBanner(payload?.Message ?? "Could not load roles.", success: false);
            }
        }
        catch (Exception ex)
        {
            roles.Clear();
            SetBanner($"Could not load roles: {ex.Message}", success: false);
        }
        finally
        {
            isLoadingList = false;
        }
    }

    private static ValidationMessageStore ValidateRoleRequest(
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
            store.Add(editContext.Field("Name"), "Role name is required");
        else if (trimmedName.Length > 100)
            store.Add(editContext.Field("Name"), "Role name must not exceed 100 characters");
        else if (!RoleNameRegex.IsMatch(trimmedName))
            store.Add(editContext.Field("Name"), "Role name can only contain letters, numbers, underscores, hyphens, and spaces");

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
