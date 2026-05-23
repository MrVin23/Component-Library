using Server.Models.Users;
using Shared.Dtos.UserPermissions;
using Shared.Dtos.Users;

namespace Server.Mapping.UserPermissions;

public static class SignUpMapper
{
    public static User ToNewUser(SignUpRequest request, string passwordHash)
    {
        var utc = DateTime.UtcNow;
        return new User
        {
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = passwordHash,
            CreatedAt = utc,
            UpdatedAt = utc
        };
    }

    public static SignUpResponse ToSignUpResponse(User user) =>
        new()
        {
            Id = user.Id,
            Username = user.Username ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Message = "Account created successfully. You can now log in."
        };
}
