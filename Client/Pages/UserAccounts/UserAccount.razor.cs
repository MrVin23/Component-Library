using Client.Components.Breadcrumbs;
using Client.Interfaces.Authorisation;
using Client.Utils.AppSettings;
using Microsoft.AspNetCore.Components;
using Shared.Dtos.Users;

namespace Client.Pages.UserAccounts;

public partial class UserAccount : ComponentBase
{
    private static readonly IReadOnlyList<BreadcrumbItem> BreadcrumbItems =
    [
        new BreadcrumbItem("Home", "/"),
        new BreadcrumbItem("Account", href: null),
    ];

    [Inject]
    private ISecureStorageService SecureStorage { get; set; } = null!;

    private ClientSession? session;
    private LoginResponse? user;
    private bool isLoading = true;
    private bool isSignedIn;

    protected override async Task OnInitializedAsync()
    {
        session = await ClientSessionStorage.ReadAsync(SecureStorage);
        user = session?.User;
        isSignedIn = user != null && !string.IsNullOrWhiteSpace(user.Username);
        isLoading = false;
    }

    private static string DisplayOrDash(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string DisplayName(LoginResponse u)
    {
        var full = $"{u.FirstName} {u.LastName}".Trim();
        return string.IsNullOrWhiteSpace(full) ? DisplayOrDash(u.Username) : full;
    }
}
