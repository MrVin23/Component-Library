namespace Shared.Dtos;

/// <summary>
/// Request token for the X-XSRF-TOKEN header; pairs with the antiforgery HttpOnly cookie.
/// </summary>
public sealed class AntiforgeryTokenResponse
{
    public string RequestToken { get; set; } = string.Empty;
}
