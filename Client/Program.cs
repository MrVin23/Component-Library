using FluentValidation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Apis;
using Client.Apis.AppSettings;
using Client.Apis.UserPermissions;
using Client.Apis.Users;
using Client.Interfaces.Authorisation;
using Client.Services.Authorisation;
using Client.Utils.AppSettings;
using Client.Utils.UserPermissions;
using Shared.Dtos.UserPermissions;
using Shared.Dtos.Users;
using Shared.Validators.UserPermissions;
using Shared.Validators.Users;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<TokenRefreshOptions>(
    builder.Configuration.GetSection(TokenRefreshOptions.SectionName));

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
var baseAddress = string.IsNullOrWhiteSpace(apiBaseUrl)
    ? builder.HostEnvironment.BaseAddress
    : apiBaseUrl.TrimEnd('/') + "/";

builder.Services.AddScoped<BrowserCredentialsHandler>();
builder.Services.AddScoped<UnauthorizedResponseHandler>();
builder.Services.AddScoped<AntiforgeryTokenStore>();
builder.Services.AddScoped<AntiforgeryHeaderHandler>();
builder.Services.AddScoped(sp =>
{
    var unauthorizedHandler = sp.GetRequiredService<UnauthorizedResponseHandler>();
    var antiforgeryHandler = sp.GetRequiredService<AntiforgeryHeaderHandler>();
    var credentialsHandler = sp.GetRequiredService<BrowserCredentialsHandler>();
    unauthorizedHandler.InnerHandler = antiforgeryHandler;
    antiforgeryHandler.InnerHandler = credentialsHandler;
    credentialsHandler.InnerHandler = new HttpClientHandler();
    return new HttpClient(unauthorizedHandler) { BaseAddress = new Uri(baseAddress) };
});
builder.Services.AddScoped<ApiAntiforgery>();
builder.Services.AddScoped<ApiUsers>();
builder.Services.AddScoped<ApiUserSettings>();
builder.Services.AddScoped<ApiAuth>();
builder.Services.AddScoped<ApiPermissionsAndRoles>();
builder.Services.AddScoped<ApiSignUpKey>();
builder.Services.AddScoped<ApiTest>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ISecureStorageService, SecureStorageService>();
builder.Services.AddScoped<ThemeHandler>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenServices, TokenServices>();
builder.Services.AddScoped<TokenRefreshService>();

builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<SignUpRequest>, SignUpRequestValidator>();
builder.Services.AddScoped<IValidator<CreateSignUpKeyRequest>, CreateSignUpKeyValidator>();

await builder.Build().RunAsync();
