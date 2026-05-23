using Client.Interfaces.Authorisation;
using Shared.Dtos.Users;

namespace Client.Utils.AppSettings;

/// <summary>
/// Reads <see cref="ClientSession"/> from secure session storage, including legacy <see cref="LoginResponse"/> blobs.
/// </summary>
public static class ClientSessionStorage
{
    public const string SessionKey = "currentUser";

    public static UserSettingsResponse DefaultSettings(int userId) =>
        new() { Id = 0, UserId = userId, DarkMode = true };

    public static async Task<ClientSession?> ReadAsync(ISecureStorageService storage)
    {
        var session = await storage.GetAsync<ClientSession>(SessionKey);
        if (session?.User != null && !string.IsNullOrWhiteSpace(session.User.Username))
            return session;

        var legacy = await storage.GetAsync<LoginResponse>(SessionKey);
        if (legacy != null && !string.IsNullOrWhiteSpace(legacy.Username))
        {
            return new ClientSession
            {
                User = legacy,
                Settings = DefaultSettings(legacy.Id)
            };
        }

        return null;
    }
}
