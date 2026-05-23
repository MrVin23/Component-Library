using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Client.Apis.UserPermissions;
using Client.Apis.Users;
using Client.Components.Breadcrumbs;
using Client.Components.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Shared.Dtos;
using Shared.Dtos.UserPermissions;
using Shared.Dtos.Users;

namespace Client.Pages.UserManagement;

public partial class UserRolesManagement : ComponentBase
{
    private static readonly IReadOnlyList<BreadcrumbItem> BreadcrumbItems =
    [
        new BreadcrumbItem("Home", "/"),
        new BreadcrumbItem("User management", "/usermanagement-dashboard"),
        new BreadcrumbItem("User roles", href: null),
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Inject] private ApiPermissionsAndRoles PermissionsApi { get; set; } = null!;
    [Inject] private ApiUsers UsersApi { get; set; } = null!;

    private Snackbar? feedbackSnackbar;

    private readonly List<UserResponse> users = new();
    private readonly List<RoleWithPermissionsResponse> roles = new();
    private readonly List<UserRoleResponse> userRoleLinks = new();
    private readonly LinkUserRoleFormModel linkRequest = new();

    private UserResponse? selectedUser;
    private RoleWithPermissionsResponse? selectedRole;
    private EditContext? linkEditContext;
    private ValidationMessageStore? linkValidationStore;

    private bool isLoadingList = true;
    private bool isSaving;
    private int? isDeletingId;
    private string bannerMessage = string.Empty;
    private bool bannerIsError;

    private string BannerCssClass =>
        bannerIsError ? "user-roles-management__banner user-roles-management__banner--error"
            : "user-roles-management__banner user-roles-management__banner--success";
    private bool IsUserInvalid => HasFieldValidationError(nameof(LinkUserRoleFormModel.UserId));
    private bool IsRoleInvalid => HasFieldValidationError(nameof(LinkUserRoleFormModel.RoleId));

    protected override async Task OnInitializedAsync()
    {
        linkEditContext = new EditContext(linkRequest);
        linkValidationStore = new ValidationMessageStore(linkEditContext);
        await ReloadUsersAndRolesAsync();
        await ReloadUserRoleLinksAsync();
    }

    private string DisplayUser(UserResponse user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
            return string.IsNullOrWhiteSpace(user.Email) ? user.Username : $"{user.Username} ({user.Email})";
        return string.IsNullOrWhiteSpace(user.Email) ? $"{fullName} ({user.Username})" : $"{fullName} ({user.Email})";
    }

    private static string FormatUserFromLink(UserRoleResponse link)
    {
        var fullName = $"{link.FirstName} {link.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
            return string.IsNullOrWhiteSpace(link.Email) ? link.Username : $"{link.Username} ({link.Email})";
        return string.IsNullOrWhiteSpace(link.Email) ? $"{fullName} ({link.Username})" : $"{fullName} ({link.Email})";
    }

    private Task OnSelectedUserChanged(UserResponse? value)
    {
        selectedUser = value;
        linkRequest.UserId = value?.Id;
        ClearFieldValidation(nameof(LinkUserRoleFormModel.UserId));
        return Task.CompletedTask;
    }

    private Task OnSelectedRoleChanged(RoleWithPermissionsResponse? value)
    {
        selectedRole = value;
        linkRequest.RoleId = value?.Id;
        ClearFieldValidation(nameof(LinkUserRoleFormModel.RoleId));
        return Task.CompletedTask;
    }

    private async Task ReloadUsersAndRolesAsync()
    {
        try
        {
            var usersResponse = await UsersApi.GetAllUsersAsync();
            if (usersResponse.IsSuccessStatusCode)
            {
                var usersPayload = await ReadApiResponseAsync<IEnumerable<UserResponse>>(usersResponse);
                if (usersPayload is { Success: true, Data: not null })
                {
                    users.Clear();
                    users.AddRange(usersPayload.Data.OrderBy(u => u.Username, StringComparer.OrdinalIgnoreCase));
                }
            }

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
        }
        catch (Exception ex)
        {
            SetBanner($"Could not load users or roles: {ex.Message}", success: false);
        }
    }

    private async Task ReloadUserRoleLinksAsync()
    {
        isLoadingList = true;
        try
        {
            var response = await PermissionsApi.GetUserRolesAsync(pageNumber: 1, pageSize: 500);
            if (!response.IsSuccessStatusCode)
            {
                userRoleLinks.Clear();
                var err = await ReadApiErrorAsync(response);
                SetBanner(err?.Message ?? "Could not load user-role links.", success: false);
                return;
            }

            var payload = await ReadPaginatedApiResponseAsync<UserRoleResponse>(response);
            if (payload is { Success: true, Data: not null })
            {
                userRoleLinks.Clear();
                userRoleLinks.AddRange(payload.Data.OrderByDescending(l => l.UpdatedAt));
            }
            else
            {
                userRoleLinks.Clear();
                SetBanner(payload?.Message ?? "Could not load user-role links.", success: false);
            }
        }
        catch (Exception ex)
        {
            userRoleLinks.Clear();
            SetBanner($"Could not load links: {ex.Message}", success: false);
        }
        finally
        {
            isLoadingList = false;
        }
    }

    private async Task HandleClearAsync(MouseEventArgs _)
    {
        selectedUser = null;
        selectedRole = null;
        linkRequest.UserId = null;
        linkRequest.RoleId = null;
        linkValidationStore?.Clear();
        linkEditContext?.NotifyValidationStateChanged();
        ClearBanner();
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleLinkSubmitAsync(EditContext editContext)
    {
        ClearBanner();
        linkEditContext = editContext;
        linkValidationStore = ValidateLinkRequest(editContext, linkRequest, linkValidationStore);
        if (editContext.GetValidationMessages().Any())
            return;

        isSaving = true;
        try
        {
            var response = await PermissionsApi.AssignRoleToUserAsync(linkRequest.UserId!.Value, linkRequest.RoleId!.Value);
            if (response.IsSuccessStatusCode)
            {
                feedbackSnackbar?.Show(
                    $"Linked role '{selectedRole?.Name}' to user '{DisplayUser(selectedUser!)}'.",
                    AlertSeverity.Success,
                    title: "Linked");
                selectedRole = null;
                linkRequest.RoleId = null;
                linkValidationStore?.Clear();
                linkEditContext?.NotifyValidationStateChanged();
                await ReloadUserRoleLinksAsync();
                return;
            }

            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict)
            {
                var err = await ReadApiErrorAsync(response);
                feedbackSnackbar?.Show(err?.Message ?? "This role is already assigned to the user.", AlertSeverity.Warning, title: "Already linked");
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

    private async Task DeleteLinkAsync(UserRoleResponse link)
    {
        ClearBanner();
        isDeletingId = link.Id;
        try
        {
            var response = await PermissionsApi.RemoveUserRoleAsync(link.UserId, link.RoleId);
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                feedbackSnackbar?.Show(
                    $"Removed role '{link.RoleName}' from user '{FormatUserFromLink(link)}'.",
                    AlertSeverity.Success,
                    title: "Removed");
                await ReloadUserRoleLinksAsync();
                return;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                SetBanner("That link was already removed.", success: false);
                await ReloadUserRoleLinksAsync();
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

    private Task DeleteLinkFromRowAsync(UserRoleResponse link) => DeleteLinkAsync(link);

    private static ValidationMessageStore ValidateLinkRequest(
        EditContext editContext,
        LinkUserRoleFormModel request,
        ValidationMessageStore? store)
    {
        store ??= new ValidationMessageStore(editContext);
        store.Clear();

        if (!request.UserId.HasValue)
            store.Add(editContext.Field(nameof(LinkUserRoleFormModel.UserId)), "User is required");

        if (!request.RoleId.HasValue)
            store.Add(editContext.Field(nameof(LinkUserRoleFormModel.RoleId)), "Role is required");

        editContext.NotifyValidationStateChanged();
        return store;
    }

    private void ClearFieldValidation(string fieldName)
    {
        if (linkEditContext is null || linkValidationStore is null)
            return;

        var field = linkEditContext.Field(fieldName);
        linkValidationStore.Clear(field);
        linkEditContext.NotifyValidationStateChanged();
    }

    private bool HasFieldValidationError(string fieldName)
    {
        if (linkEditContext is null)
            return false;

        var field = linkEditContext.Field(fieldName);
        return linkEditContext.GetValidationMessages(field).Any();
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

    private sealed class LinkUserRoleFormModel
    {
        public int? UserId { get; set; }
        public int? RoleId { get; set; }
    }
}
