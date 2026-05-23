using System.Security.Claims;
using Client.Interfaces.Authorisation;
using Client.Utils.AppSettings;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Dtos.Users;

namespace Client.Utils.UserPermissions
{
    public sealed class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ISecureStorageService _secureStorage;
        private AuthenticationState? _cachedState;
        private DateTime? _cacheTimestamp;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(5); // Cache for 5 seconds

        public CustomAuthStateProvider(ISecureStorageService secureStorage)
        {
            _secureStorage = secureStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Use cached state if available and not expired
            if (_cachedState != null && _cacheTimestamp.HasValue)
            {
                var cacheAge = DateTime.UtcNow - _cacheTimestamp.Value;
                if (cacheAge < _cacheExpiration)
                {
                    return _cachedState;
                }
            }

            // Cache expired or not set, read from storage
            try
            {
                var session = await ClientSessionStorage.ReadAsync(_secureStorage);
                var user = session?.User;

                if (user != null && !string.IsNullOrWhiteSpace(user.Username))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username.Trim()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    if (!string.IsNullOrWhiteSpace(user.FirstName))
                        claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));

                    if (!string.IsNullOrWhiteSpace(user.LastName))
                        claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

                    if (!string.IsNullOrWhiteSpace(user.Email))
                        claims.Add(new Claim(ClaimTypes.Email, user.Email));

                    // Add role claims for AuthorizeView to work with roles
                    if (user.Roles != null && user.Roles.Count > 0)
                    {
                        foreach (var role in user.Roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                    }

                    var identity = new ClaimsIdentity(claims, "CustomAuth");
                    var principal = new ClaimsPrincipal(identity);
                    
                    _cachedState = new AuthenticationState(principal);
                    _cacheTimestamp = DateTime.UtcNow;
                    return _cachedState;
                }
            }
            catch
            {
                // If there's any error reading the user, return anonymous
            }

            _cachedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            _cacheTimestamp = DateTime.UtcNow;
            return _cachedState;
        }

        public void NotifyAuthenticationStateChanged()
        {
            // Clear cache when authentication state changes
            _cachedState = null;
            _cacheTimestamp = null;
            base.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
