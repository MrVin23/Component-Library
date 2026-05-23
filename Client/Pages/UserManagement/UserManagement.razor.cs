using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Client.Apis.Users;
using Client.Components.Breadcrumbs;
using Client.Components.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Shared.Dtos;
using Shared.Dtos.Users;

namespace Client.Pages.UserManagement;

public partial class UserManagement : ComponentBase
{
    private static readonly IReadOnlyList<BreadcrumbItem> BreadcrumbItems =
    [
        new BreadcrumbItem("Home", "/"),
        new BreadcrumbItem("User management", "/usermanagement-dashboard"),
        new BreadcrumbItem("Users", href: null),
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [Inject] private ApiUsers UsersApi { get; set; } = null!;

    private Snackbar? feedbackSnackbar;

    private readonly List<UserResponse> allUsers = new();
    private readonly List<UserResponse> filteredUsers = new();
    private readonly UserTableFilterModel userFilterDraft = new();
    private readonly UserTableFilterModel userFilterApplied = new();
    private CreateUserRequest createRequest = NewCreateRequest();

    private EditContext? createEditContext;
    private ValidationMessageStore? createValidationStore;

    private bool isLoadingList = true;
    private bool isSaving;
    private int? isDeletingId;
    private int? editingUserId;
    private bool highlightPrimaryForm;
    private string bannerMessage = string.Empty;
    private bool bannerIsError;

    private string BannerCssClass =>
        bannerIsError ? "users-management__banner users-management__banner--error"
            : "users-management__banner users-management__banner--success";
    private string PrimaryFormTitle => editingUserId.HasValue ? "Edit user" : "Create user";

    private string UsersTableEmptyMessage =>
        allUsers.Count == 0 ? "No users found." : "No users match your filter.";

    protected override async Task OnInitializedAsync() => await ReloadUsersAsync();

    /// <summary>
    /// <see cref="InputText"/> uses <c>CaptureUnmatchedValues</c> for extra attributes.
    /// Do not pass <c>class</c> / <c>placeholder</c> as separate markup attributes when also setting <c>AdditionalAttributes</c>.
    /// </summary>
    private IReadOnlyDictionary<string, object> GetPasswordInputAttributes() =>
        new Dictionary<string, object>
        {
            ["type"] = "password",
            ["autocomplete"] = "new-password",
            ["class"] = "app-input",
            ["placeholder"] = editingUserId.HasValue
                ? "Leave blank to keep current password"
                : "Required for new users",
        };

    private static CreateUserRequest NewCreateRequest() => new()
    {
        Username = string.Empty,
        Email = string.Empty,
        FirstName = string.Empty,
        LastName = string.Empty,
        Password = string.Empty
    };

    private async Task HandleCreateResetAsync(MouseEventArgs _)
    {
        editingUserId = null;
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
        createValidationStore = ValidateUserRequest(editContext, createRequest, editingUserId, createValidationStore);
        if (editContext.GetValidationMessages().Any())
            return;

        createRequest.Username = createRequest.Username.Trim();
        createRequest.Email = createRequest.Email.Trim();
        createRequest.FirstName = createRequest.FirstName?.Trim() ?? string.Empty;
        createRequest.LastName = createRequest.LastName?.Trim() ?? string.Empty;

        isSaving = true;
        try
        {
            if (editingUserId.HasValue)
            {
                var updateRequest = new UpdateUserRequest
                {
                    Username = createRequest.Username,
                    Email = createRequest.Email,
                    FirstName = createRequest.FirstName,
                    LastName = createRequest.LastName,
                    Password = string.IsNullOrWhiteSpace(createRequest.Password) ? null : createRequest.Password
                };

                var updateResponse = await UsersApi.UpdateUserAsync(editingUserId.Value, updateRequest);
                if (updateResponse.IsSuccessStatusCode)
                {
                    var payload = await ReadApiResponseAsync<UserResponse>(updateResponse);
                    var displayName = payload?.Data?.Username ?? updateRequest.Username;
                    feedbackSnackbar?.Show($"User '{displayName}' updated.", AlertSeverity.Success, title: "Updated");
                    editingUserId = null;
                    createRequest = NewCreateRequest();
                    await ReloadUsersAsync();
                    return;
                }

                if (updateResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    SetBanner("That user no longer exists.", success: false);
                    editingUserId = null;
                    createRequest = NewCreateRequest();
                    await ReloadUsersAsync();
                    return;
                }

                var updateErr = await ReadApiErrorAsync(updateResponse);
                SetBanner(updateErr?.Message ?? updateResponse.ReasonPhrase ?? "Could not update user.", success: false);
                return;
            }

            var response = await UsersApi.CreateUserAsync(createRequest);
            if (response.IsSuccessStatusCode)
            {
                var payload = await ReadApiResponseAsync<UserResponse>(response);
                if (payload is { Success: true, Data: not null })
                {
                    feedbackSnackbar?.Show($"User '{payload.Data.Username}' created.", AlertSeverity.Success, title: "Created");
                    createRequest = NewCreateRequest();
                    await ReloadUsersAsync();
                    return;
                }

                SetBanner(payload?.Message ?? "Could not create user.", success: false);
                return;
            }

            var err = await ReadApiErrorAsync(response);
            SetBanner(err?.Message ?? response.ReasonPhrase ?? "Could not create user.", success: false);
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

    private void StartEdit(UserResponse user)
    {
        ClearBanner();
        editingUserId = user.Id;
        createRequest = new CreateUserRequest
        {
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Password = string.Empty
        };
        createValidationStore?.Clear();
        createEditContext?.NotifyValidationStateChanged();
        _ = HighlightPrimaryFormAsync();
    }

    private Task StartEditFromRowAsync(UserResponse user)
    {
        StartEdit(user);
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

    private async Task DeleteUserAsync(int id)
    {
        ClearBanner();
        isDeletingId = id;
        var removedName = allUsers.FirstOrDefault(u => u.Id == id)?.Username;
        try
        {
            var response = await UsersApi.DeleteUserAsync(id);
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                feedbackSnackbar?.Show(
                    string.IsNullOrWhiteSpace(removedName) ? "User deleted." : $"User '{removedName}' deleted.",
                    AlertSeverity.Success,
                    title: "Deleted");

                if (editingUserId == id)
                {
                    editingUserId = null;
                    createRequest = NewCreateRequest();
                }

                await ReloadUsersAsync();
                return;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                SetBanner("That user was already removed.", success: false);
                await ReloadUsersAsync();
                return;
            }

            var err = await ReadApiErrorAsync(response);
            SetBanner(err?.Message ?? response.ReasonPhrase ?? "Could not delete user.", success: false);
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

    private Task DeleteUserFromRowAsync(UserResponse user) => DeleteUserAsync(user.Id);

    private async Task ReloadUsersAsync()
    {
        isLoadingList = true;
        try
        {
            var response = await UsersApi.GetAllUsersAsync();
            if (!response.IsSuccessStatusCode)
            {
                allUsers.Clear();
                RebuildFilteredUsers();
                var err = await ReadApiErrorAsync(response);
                SetBanner(err?.Message ?? "Could not load users.", success: false);
                return;
            }

            var payload = await ReadApiResponseAsync<IEnumerable<UserResponse>>(response);
            if (payload is { Success: true, Data: not null })
            {
                allUsers.Clear();
                allUsers.AddRange(payload.Data.OrderByDescending(u => u.CreatedAt));
                RebuildFilteredUsers();
            }
            else
            {
                allUsers.Clear();
                RebuildFilteredUsers();
                SetBanner(payload?.Message ?? "Could not load users.", success: false);
            }
        }
        catch (Exception ex)
        {
            allUsers.Clear();
            RebuildFilteredUsers();
            SetBanner($"Could not load users: {ex.Message}", success: false);
        }
        finally
        {
            isLoadingList = false;
        }
    }

    private Task OnUserListFilterPanelExpandedAsync(bool expanded)
    {
        if (expanded)
            UserTableFilterModel.CopyFrom(userFilterApplied, userFilterDraft);
        return Task.CompletedTask;
    }

    private Task HandleUserListFilterSearchAsync(MouseEventArgs _)
    {
        UserTableFilterModel.CopyFrom(userFilterDraft, userFilterApplied);
        RebuildFilteredUsers();
        return Task.CompletedTask;
    }

    private Task HandleUserListFilterCancelAsync(MouseEventArgs _)
    {
        userFilterDraft.Clear();
        userFilterApplied.Clear();
        RebuildFilteredUsers();
        return Task.CompletedTask;
    }

    private void RebuildFilteredUsers()
    {
        filteredUsers.Clear();
        if (!HasAnyAppliedUserFilters())
        {
            filteredUsers.AddRange(allUsers);
            return;
        }

        filteredUsers.AddRange(allUsers.Where(UserMatchesAppliedFilters));
    }

    private bool HasAnyAppliedUserFilters()
    {
        return !string.IsNullOrWhiteSpace(userFilterApplied.Username)
               || !string.IsNullOrWhiteSpace(userFilterApplied.Email)
               || !string.IsNullOrWhiteSpace(userFilterApplied.FirstName)
               || !string.IsNullOrWhiteSpace(userFilterApplied.LastName);
    }

    private bool UserMatchesAppliedFilters(UserResponse u)
    {
        if (!string.IsNullOrWhiteSpace(userFilterApplied.Username)
            && !FieldContains(u.Username, userFilterApplied.Username))
            return false;
        if (!string.IsNullOrWhiteSpace(userFilterApplied.Email)
            && !FieldContains(u.Email, userFilterApplied.Email))
            return false;
        if (!string.IsNullOrWhiteSpace(userFilterApplied.FirstName)
            && !FieldContains(u.FirstName, userFilterApplied.FirstName))
            return false;
        if (!string.IsNullOrWhiteSpace(userFilterApplied.LastName)
            && !FieldContains(u.LastName, userFilterApplied.LastName))
            return false;
        return true;

        static bool FieldContains(string? haystack, string needle)
        {
            var n = needle.Trim();
            return !string.IsNullOrEmpty(haystack) && haystack.Contains(n, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static ValidationMessageStore ValidateUserRequest(
        EditContext editContext,
        CreateUserRequest request,
        int? editingUserId,
        ValidationMessageStore? store)
    {
        store ??= new ValidationMessageStore(editContext);
        store.Clear();

        var username = request.Username?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var first = request.FirstName?.Trim() ?? string.Empty;
        var last = request.LastName?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username))
            store.Add(editContext.Field(nameof(CreateUserRequest.Username)), "Username is required");
        else if (username.Length > 100)
            store.Add(editContext.Field(nameof(CreateUserRequest.Username)), "Username must not exceed 100 characters");

        if (string.IsNullOrWhiteSpace(email))
            store.Add(editContext.Field(nameof(CreateUserRequest.Email)), "Email is required");
        else if (email.Length > 256)
            store.Add(editContext.Field(nameof(CreateUserRequest.Email)), "Email must not exceed 256 characters");
        else if (!EmailRegex.IsMatch(email))
            store.Add(editContext.Field(nameof(CreateUserRequest.Email)), "Enter a valid email address");

        if (first.Length > 100)
            store.Add(editContext.Field(nameof(CreateUserRequest.FirstName)), "First name must not exceed 100 characters");
        if (last.Length > 100)
            store.Add(editContext.Field(nameof(CreateUserRequest.LastName)), "Last name must not exceed 100 characters");

        if (!editingUserId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(password))
                store.Add(editContext.Field(nameof(CreateUserRequest.Password)), "Password is required");
            else if (password.Length < 8)
                store.Add(editContext.Field(nameof(CreateUserRequest.Password)), "Password must be at least 8 characters");
        }
        else if (!string.IsNullOrWhiteSpace(password) && password.Length < 8)
        {
            store.Add(editContext.Field(nameof(CreateUserRequest.Password)), "Password must be at least 8 characters");
        }

        editContext.NotifyValidationStateChanged();
        return store;
    }

    private static string FormatFullName(UserResponse user)
    {
        var full = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(full) ? "-" : full;
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

    private sealed class UserTableFilterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public void Clear()
        {
            Username = string.Empty;
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
        }

        public static void CopyFrom(UserTableFilterModel source, UserTableFilterModel target)
        {
            target.Username = source.Username ?? string.Empty;
            target.Email = source.Email ?? string.Empty;
            target.FirstName = source.FirstName ?? string.Empty;
            target.LastName = source.LastName ?? string.Empty;
        }
    }
}
