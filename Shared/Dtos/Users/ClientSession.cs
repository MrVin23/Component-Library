namespace Shared.Dtos.Users;

/// <summary>
/// Encrypted session payload stored under <c>currentUser</c> in session storage (auth + cached user settings).
/// </summary>
public sealed class ClientSession
{
    public LoginResponse User { get; set; } = null!;
    public UserSettingsResponse? Settings { get; set; }
}
