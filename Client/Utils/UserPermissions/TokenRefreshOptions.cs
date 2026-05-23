namespace Client.Utils.UserPermissions;

public class TokenRefreshOptions
{
    public const string SectionName = "TokenRefresh";

    /// <summary>How often the client checks whether the token should be refreshed (seconds, default 2 minutes).</summary>
    public int CheckIntervalSeconds { get; set; } = 120;
}
