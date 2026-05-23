using System.Net;
using Client.Interfaces.Authorisation;
using Client.Utils.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Utils.UserPermissions;

/// <summary>
/// Intercepts HTTP responses and redirects to login when a 401 Unauthorized is received.
/// Skips auth endpoints where 401 is expected (login, signup, etc.).
/// </summary>
public class UnauthorizedResponseHandler : DelegatingHandler
{
    private static volatile bool _redirectInProgress;

    private static readonly string[] SkipPaths =
    [
        "api/auth/login",
        "api/auth/signup",
        "api/auth/logout",
        "api/auth/me",
        "api/auth/check-username",
        "api/auth/check-email"
    ];

    private readonly NavigationManager _navigation;
    private readonly ISecureStorageService _secureStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public UnauthorizedResponseHandler(
        NavigationManager navigation,
        ISecureStorageService secureStorage,
        AuthenticationStateProvider authStateProvider)
    {
        _navigation = navigation;
        _secureStorage = secureStorage;
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        var requestUri = request.RequestUri?.ToString() ?? string.Empty;
        if (ShouldSkipRedirect(requestUri))
            return response;

        await HandleUnauthorizedAsync();
        return response;
    }

    private static bool ShouldSkipRedirect(string requestUri)
    {
        var path = requestUri;
        var queryIndex = path.IndexOf('?');
        if (queryIndex >= 0)
            path = path[..queryIndex];

        return SkipPaths.Any(skip => path.Contains(skip, StringComparison.OrdinalIgnoreCase));
    }

    private async Task HandleUnauthorizedAsync()
    {
        if (_redirectInProgress)
            return;

        _redirectInProgress = true;
        try
        {
            await _secureStorage.RemoveAsync(ClientSessionStorage.SessionKey);

            if (_authStateProvider is CustomAuthStateProvider customProvider)
                customProvider.NotifyAuthenticationStateChanged();

            _navigation.NavigateTo("/login", forceLoad: true);
        }
        catch
        {
            // Best effort - redirect even if other cleanup fails
            _navigation.NavigateTo("/login", forceLoad: true);
        }
    }
}
