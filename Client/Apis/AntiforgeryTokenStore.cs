namespace Client.Apis;

/// <summary>
/// Holds the antiforgery request token for the X-XSRF-TOKEN header (paired with the HttpOnly antiforgery cookie).
/// </summary>
public sealed class AntiforgeryTokenStore
{
    public string? RequestToken { get; set; }
}
