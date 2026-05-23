namespace Client.Apis;

/// <summary>
/// Adds the antiforgery request token header for unsafe HTTP methods (SPA + credentialed API).
/// </summary>
public sealed class AntiforgeryHeaderHandler(AntiforgeryTokenStore tokenStore) : DelegatingHandler
{
    public const string HeaderName = "X-XSRF-TOKEN";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (RequiresAntiforgeryHeader(request.Method) &&
            !string.IsNullOrEmpty(tokenStore.RequestToken))
        {
            request.Headers.TryAddWithoutValidation(HeaderName, tokenStore.RequestToken);
        }

        return base.SendAsync(request, cancellationToken);
    }

    private static bool RequiresAntiforgeryHeader(HttpMethod method) =>
        method == HttpMethod.Post
        || method == HttpMethod.Put
        || method == HttpMethod.Delete
        || method == HttpMethod.Patch;
}
