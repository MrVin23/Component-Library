using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Client.Apis;

/// <summary>
/// Ensures browser <c>fetch</c> sends cookies on each request (needed for API auth cookies when the API is another origin).
/// </summary>
public sealed class BrowserCredentialsHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}
