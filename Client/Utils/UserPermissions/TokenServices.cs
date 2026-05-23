using System.Security.Claims;
using System.Text.Json;
using Client.Interfaces.Authorisation;

namespace Client.Utils.UserPermissions
{
    public class TokenServices : ITokenServices
    {
        private readonly ISecureStorageService _secureStorage;

        public TokenServices(ISecureStorageService secureStorage)
        {
            _secureStorage = secureStorage;
        }

        /// <summary>
        /// Extracts and parses claims from a JWT token.
        /// This method decodes the JWT payload and converts it into .NET Claims objects
        /// that can be used for authorization.
        /// </summary>
        /// <param name="jwt">The JWT token string</param>
        /// <returns>A collection of Claims representing user identity and permissions</returns>
        public IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                throw new ArgumentNullException(nameof(jwt), "JWT token cannot be null or empty");
            }

            var claims = new List<Claim>();
            var parts = jwt.Split('.');
            if (parts.Length != 3)
            {
                throw new ArgumentException("Invalid JWT token format", nameof(jwt));
            }

            var payload = parts[1];

            // Decode the Base64Url encoded payload
            var jsonBytes = ParseBase64WithoutPadding(payload);
            // Convert JSON to Dictionary
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes)
                ?? throw new InvalidOperationException("Failed to deserialize JWT payload");

            // Special handling for roles since they can be either a single value or an array
            keyValuePairs.TryGetValue(ClaimTypes.Role, out object? roles);

            if (roles != null)
            {
                var rolesString = roles.ToString();
                if (string.IsNullOrEmpty(rolesString))
                {
                    // Skip processing if roles string is empty
                    keyValuePairs.Remove(ClaimTypes.Role);
                }
                else if (rolesString.Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(rolesString)
                        ?? Array.Empty<string>();

                    foreach (var parsedRole in parsedRoles)
                    {
                        if (!string.IsNullOrEmpty(parsedRole))
                        {
                            claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                        }
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, rolesString));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            // Convert remaining key-value pairs to claims
            claims.AddRange(keyValuePairs.Select(kvp =>
                new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty)));

            return claims;
        }

        /// <summary>
        /// Helper method to properly handle Base64 strings that might be missing padding.
        /// JWT tokens use Base64Url encoding which may omit the padding characters,
        /// so we need to add them back before decoding.
        /// </summary>
        /// <param name="base64">The Base64 string to parse</param>
        /// <returns>Decoded bytes from the Base64 string</returns>
        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            // Base64 strings must have a length that's a multiple of 4
            // Add padding characters ('=') as needed
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;  // Add two padding characters
                case 3: base64 += "="; break;   // Add one padding character
            }
            return Convert.FromBase64String(base64);
        }

        // SECTION: Token Storage Methods

        /// <summary>
        /// Stores the role token in secure storage
        /// </summary>
        /// <param name="roleToken">The JWT token containing user roles</param>
        public async Task StoreRoleToken(string roleToken)
        {
            await _secureStorage.SetAsync("roleToken", roleToken);
        }

        /// <summary>
        /// Retrieves the stored role token from secure storage
        /// </summary>
        /// <returns>The stored JWT token or null if not found</returns>
        public async Task<string?> GetRoleToken()
        {
            return await _secureStorage.GetAsync<string>("roleToken");
        }

        /// <summary>
        /// Removes the role token from secure storage
        /// Typically used during logout
        /// </summary>
        public async Task RemoveRoleToken()
        {
            await _secureStorage.RemoveAsync("roleToken");
        }
    }
}