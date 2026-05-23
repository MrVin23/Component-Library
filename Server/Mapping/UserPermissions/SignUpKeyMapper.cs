using Server.Models.UserPermissions;
using Shared.Dtos.UserPermissions;

namespace Server.Mapping.UserPermissions;

public static class SignUpKeyMapper
{
    public static SignUpKey ToNewEntity(string key, DateTime expiresAt)
    {
        var utc = DateTime.UtcNow;
        return new SignUpKey
        {
            Key = key,
            ExpiresAt = expiresAt,
            CreatedAt = utc,
            UpdatedAt = utc
        };
    }

    public static SignUpKeyResponse ToResponse(SignUpKey signUpKey)
    {
        var now = DateTime.UtcNow;
        var isExpired = signUpKey.ExpiresAt < now;

        return new SignUpKeyResponse
        {
            Id = signUpKey.Id,
            Key = signUpKey.Key,
            ExpiresAt = signUpKey.ExpiresAt,
            CreatedAt = signUpKey.CreatedAt.UtcDateTime,
            UpdatedAt = signUpKey.UpdatedAt.UtcDateTime,
            IsExpired = isExpired,
            IsActive = !isExpired
        };
    }
}
