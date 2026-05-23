using System.Net.Http.Json;
using System.Net.Http;
using System.Security.Claims;
using Client.Apis;
using Client.Apis.AppSettings;
using Client.Interfaces.Authorisation;
using Client.Utils.AppSettings;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Dtos;
using Shared.Dtos.Users;

namespace Client.Utils.UserPermissions
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ISecureStorageService _secureStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ApiAntiforgery _apiAntiforgery;
        private readonly AntiforgeryTokenStore _antiforgeryTokens;
        private readonly ApiUserSettings _apiUserSettings;
        private readonly ThemeHandler _themeHandler;

        public AuthService(
            HttpClient httpClient,
            ISecureStorageService secureStorage,
            AuthenticationStateProvider authStateProvider,
            ApiAntiforgery apiAntiforgery,
            AntiforgeryTokenStore antiforgeryTokens,
            ApiUserSettings apiUserSettings,
            ThemeHandler themeHandler)
        {
            _httpClient = httpClient;
            _secureStorage = secureStorage;
            _authStateProvider = authStateProvider;
            _apiAntiforgery = apiAntiforgery;
            _antiforgeryTokens = antiforgeryTokens;
            _apiUserSettings = apiUserSettings;
            _themeHandler = themeHandler;
        }

        /// <summary>
        /// Re-fetches the antiforgery request token so it stays paired with the HttpOnly cookie after auth transitions.
        /// </summary>
        private async Task RefreshAntiforgeryFromServerAsync()
        {
            try
            {
                var dto = await _apiAntiforgery.GetRequestTokenAsync();
                _antiforgeryTokens.RequestToken = string.IsNullOrEmpty(dto?.RequestToken)
                    ? null
                    : dto.RequestToken;
            }
            catch
            {
                _antiforgeryTokens.RequestToken = null;
            }
        }

        /// <summary>
        /// Login with username and password
        /// </summary>
        public async Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
                {
                    Content = JsonContent.Create(request)
                };
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    // Server uses HTTP-only cookies for authentication
                    // Cookie is automatically set and will be sent with subsequent requests
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
                    await RefreshAntiforgeryFromServerAsync();
                    return apiResponse;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = error?.Message ?? "Invalid credentials",
                        Data = null
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = error?.Message ?? "Bad request",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = $"Request failed with status: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        public async Task<ApiResponse<SignUpResponse>?> SignUpAsync(SignUpRequest request)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/signup")
                {
                    Content = JsonContent.Create(request)
                };
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<SignUpResponse>>();
                    await RefreshAntiforgeryFromServerAsync();
                    return apiResponse;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    return new ApiResponse<SignUpResponse>
                    {
                        Success = false,
                        Message = error?.Message ?? "Username or email already exists",
                        Data = null
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    return new ApiResponse<SignUpResponse>
                    {
                        Success = false,
                        Message = error?.Message ?? "Validation failed",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<SignUpResponse>
                    {
                        Success = false,
                        Message = $"Request failed with status: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<SignUpResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Check if a username is available for registration
        /// </summary>
        public async Task<ApiResponse<bool>?> CheckUsernameAvailabilityAsync(string username)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"api/auth/check-username/{Uri.EscapeDataString(username)}");
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                }
                else
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Request failed with status: {response.StatusCode}",
                        Data = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = false
                };
            }
        }

        /// <summary>
        /// Check if an email is available for registration
        /// </summary>
        public async Task<ApiResponse<bool>?> CheckEmailAvailabilityAsync(string email)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"api/auth/check-email/{Uri.EscapeDataString(email)}");
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                }
                else
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Request failed with status: {response.StatusCode}",
                        Data = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = false
                };
            }
        }

        /// <summary>
        /// Logout the current user
        /// </summary>
        public async Task<ApiResponse<object>?> LogoutAsync()
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    await RefreshAntiforgeryFromServerAsync();
                    return apiResponse;
                }

                await RefreshAntiforgeryFromServerAsync();
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Logout failed with status: {response.StatusCode}",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                await RefreshAntiforgeryFromServerAsync();
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        public async Task<ApiResponse<LoginResponse>?> GetCurrentUserAsync()
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = error?.Message ?? "Not authenticated",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = $"Request failed with status: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get the current token status including expiration info
        /// </summary>
        public async Task<ApiResponse<TokenStatusResponse>?> GetTokenStatusAsync()
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/auth/token-status");
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<TokenStatusResponse>>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new ApiResponse<TokenStatusResponse>
                    {
                        Success = false,
                        Message = "Not authenticated",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<TokenStatusResponse>
                    {
                        Success = false,
                        Message = $"Request failed with status: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<TokenStatusResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Refresh the authentication token (extends session)
        /// </summary>
        public async Task<ApiResponse<TokenStatusResponse>?> RefreshTokenAsync()
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh");
                httpRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await _httpClient.SendAsync(httpRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<TokenStatusResponse>>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new ApiResponse<TokenStatusResponse>
                    {
                        Success = false,
                        Message = "Not authenticated - please log in again",
                        Data = null
                    };
                }
                else
                {
                    return new ApiResponse<TokenStatusResponse>
                    {
                        Success = false,
                        Message = $"Token refresh failed with status: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<TokenStatusResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Refreshes client-side auth state based on server cookie session.
        /// </summary>
        public async Task RefreshClientAuthStateAsync()
        {
            try
            {
                var currentUserResponse = await GetCurrentUserAsync();

                if (currentUserResponse?.Success == true && currentUserResponse.Data != null)
                {
                    var user = currentUserResponse.Data;
                    UserSettingsResponse? settingsPayload = null;
                    var settingsHttp = await _apiUserSettings.GetMineAsync();
                    if (settingsHttp.IsSuccessStatusCode)
                    {
                        var parsed = await settingsHttp.Content.ReadFromJsonAsync<ApiResponse<UserSettingsResponse>>();
                        if (parsed is { Success: true, Data: not null })
                            settingsPayload = parsed.Data;
                    }

                    var clientSession = new ClientSession
                    {
                        User = user,
                        Settings = settingsPayload ?? ClientSessionStorage.DefaultSettings(user.Id)
                    };
                    await _secureStorage.SetAsync(ClientSessionStorage.SessionKey, clientSession);
                }
                else
                {
                    await _secureStorage.RemoveAsync(ClientSessionStorage.SessionKey);
                }

                await _themeHandler.ApplyFromStoredSessionAsync();
            }
            catch
            {
                await _secureStorage.RemoveAsync(ClientSessionStorage.SessionKey);
                await _themeHandler.ApplyFromStoredSessionAsync();
            }
            finally
            {
                if (_authStateProvider is CustomAuthStateProvider customProvider)
                    customProvider.NotifyAuthenticationStateChanged();
            }
        }

        /// <summary>
        /// Checks if current authenticated user has the specified role.
        /// </summary>
        public async Task<bool> IsInRoleAsync(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated != true)
                return false;

            return user.Claims.Any(c =>
                c.Type == ClaimTypes.Role &&
                string.Equals(c.Value, role.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if current authenticated user has at least one role in the provided list.
        /// </summary>
        public async Task<bool> IsInAnyRoleAsync(params string[] roles)
        {
            if (roles == null || roles.Length == 0)
                return false;

            var normalizedRoles = roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (normalizedRoles.Count == 0)
                return false;

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated != true)
                return false;

            return user.Claims.Any(c =>
                c.Type == ClaimTypes.Role &&
                normalizedRoles.Contains(c.Value));
        }
    }
}

