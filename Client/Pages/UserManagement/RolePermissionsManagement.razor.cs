using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Client.Apis.UserPermissions;
using Client.Components.Breadcrumbs;
using Client.Components.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Shared.Dtos;
using Shared.Dtos.UserPermissions;

namespace Client.Pages.UserManagement;

public partial class RolePermissionsManagement : ComponentBase
{
    private static readonly IReadOnlyList<BreadcrumbItem> BreadcrumbItems =
    [
        new BreadcrumbItem("Home", "/"),
        new BreadcrumbItem("User management", "/usermanagement-dashboard"),
        new BreadcrumbItem("Role permissions", href: null),
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Inject] private ApiPermissionsAndRoles PermissionsApi { get; set; } = null!;

    private Snackbar? feedbackSnackbar;

    private readonly List<RoleWithPermissionsResponse> roles = new();
    private readonly List<PermissionResponse> permissions = new();
    private readonly List<RolePermissionResponse> rolePermissionLinks = new();

    private RoleWithPermissionsResponse? selectedRole;
    private PermissionResponse? selectedPermission;

    private bool isLoadingList = true;
    private bool isSaving;
    private int? isDeletingId;
    private string bannerMessage = string.Empty;
    private bool bannerIsError;

    private string BannerCssClass =>
        bannerIsError ? "role-permissions-management__banner role-permissions-management__banner--error"
            : "role-permissions-management__banner role-permissions-management__banner--success";

    protected override async Task OnInitializedAsync()
    {
        await ReloadRolesAndPermissionsAsync();
        await ReloadRolePermissionLinksAsync();
    }

    private Task OnSelectedRoleChanged(RoleWithPermissionsResponse? value)
    {
        selectedRole = value;
        return Task.CompletedTask;
    }

    private Task OnSelectedPermissionChanged(PermissionResponse? value)
    {
        selectedPermission = value;
        return Task.CompletedTask;
    }

    private async Task ReloadRolesAndPermissionsAsync()
    {
        try
        {
            var rolesResponse = await PermissionsApi.GetRolesAsync(pageNumber: 1, pageSize: 500);
            if (rolesResponse.IsSuccessStatusCode)
            {
                var rolesPayload = await ReadPaginatedApiResponseAsync<RoleWithPermissionsResponse>(rolesResponse);
                if (rolesPayload is { Success: true, Data: not null })
                {
                    roles.Clear();
                    roles.AddRange(rolesPayload.Data.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase));
                }
            }

            var permResponse = await PermissionsApi.GetPermissionsAsync(pageNumber: 1, pageSize: 500);
            if (permResponse.IsSuccessStatusCode)
            {
                var permPayload = await ReadPaginatedApiResponseAsync<PermissionResponse>(permResponse);
                if (permPayload is { Success: true, Data: not null })
                {
                    permissions.Clear();
                    permissions.AddRange(permPayload.Data.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase));
                }
            }
        }
        catch (Exception ex)
        {
            SetBanner($"Could not load roles or permissions: {ex.Message}", success: false);
        }
    }

    private async Task ReloadRolePermissionLinksAsync()
    {
        isLoadingList = true;
        try
        {
            var response = await PermissionsApi.GetRolePermissionsAsync(pageNumber: 1, pageSize: 500);
            if (!response.IsSuccessStatusCode)
            {
                rolePermissionLinks.Clear();
                var err = await ReadApiErrorAsync(response);
                SetBanner(err?.Message ?? "Could not load role–permission links.", success: false);
                return;
            }

            var payload = await ReadPaginatedApiResponseAsync<RolePermissionResponse>(response);
            if (payload is { Success: true, Data: not null })
            {
                rolePermissionLinks.Clear();
                rolePermissionLinks.AddRange(payload.Data.OrderByDescending(l => l.UpdatedAt));
            }
            else
            {
                rolePermissionLinks.Clear();
                SetBanner(payload?.Message ?? "Could not load role–permission links.", success: false);
            }
        }
        catch (Exception ex)
        {
            rolePermissionLinks.Clear();
            SetBanner($"Could not load links: {ex.Message}", success: false);
        }
        finally
        {
            isLoadingList = false;
        }
    }

    private async Task HandleClearAsync(MouseEventArgs _)
    {
        selectedRole = null;
        selectedPermission = null;
        ClearBanner();
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleLinkAsync(MouseEventArgs _)
    {
        ClearBanner();
        if (selectedRole is null || selectedPermission is null)
        {
            feedbackSnackbar?.Show("Select both a role and a permission before linking.", AlertSeverity.Warning, title: "Incomplete");
            return;
        }

        isSaving = true;
        try
        {
            var response = await PermissionsApi.AssignPermissionToRoleAsync(selectedRole.Id, selectedPermission.Id);
            if (response.IsSuccessStatusCode)
            {
                var payload = await ReadApiResponseAsync<RolePermissionResponse>(response);
                var msg = payload?.Data is { } row
                    ? $"Linked permission '{row.PermissionName}' to role '{row.RoleName}'."
                    : "Permission linked to role.";
                feedbackSnackbar?.Show(msg, AlertSeverity.Success, title: "Linked");
                selectedPermission = null;
                await ReloadRolePermissionLinksAsync();
                return;
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var err = await ReadApiErrorAsync(response);
                feedbackSnackbar?.Show(err?.Message ?? "This permission is already assigned to the role.", AlertSeverity.Warning, title: "Already linked");
                return;
            }

            var apiErr = await ReadApiErrorAsync(response);
            SetBanner(apiErr?.Message ?? response.ReasonPhrase ?? "Could not create link.", success: false);
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

    private async Task DeleteLinkAsync(RolePermissionResponse link)
    {
        ClearBanner();
        isDeletingId = link.Id;
        try
        {
            var response = await PermissionsApi.RemovePermissionFromRoleAsync(link.RoleId, link.PermissionId);
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                feedbackSnackbar?.Show(
                    $"Removed '{link.PermissionName}' from role '{link.RoleName}'.",
                    AlertSeverity.Success,
                    title: "Removed");
                await ReloadRolePermissionLinksAsync();
                return;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                SetBanner("That link was already removed.", success: false);
                await ReloadRolePermissionLinksAsync();
                return;
            }

            var err = await ReadApiErrorAsync(response);
            SetBanner(err?.Message ?? response.ReasonPhrase ?? "Could not remove link.", success: false);
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

    private Task DeleteLinkFromRowAsync(RolePermissionResponse link) => DeleteLinkAsync(link);

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
