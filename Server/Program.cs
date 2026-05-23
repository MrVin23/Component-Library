using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Server.BusinessLogic.Interfaces.UserPermissions;
using Server.BusinessLogic.Services.UserPermissions;
using Server.BusinessLogic.Services.Users;
using Server.Database;
using Server.Database.Interfaces;
using Server.Interfaces;
using Server.Middleware;
using Server.Repositories;
using Server.Repositories.Interfaces;
using Server.Repositories.Interfaces.UserPermissions;
using Server.Repositories.Interfaces.AppSettings;
using Server.Repositories.Interfaces.Users;
using Server.Repositories.Services;
using Server.Repositories.Services.UserPermissions;
using Server.Repositories.Services.AppSettings;
using Server.Repositories.Services.Users;
using Server.Utils.UserPermissions;
using Shared.Validators.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IDatabaseContext>(sp => sp.GetRequiredService<DatabaseContext>());

// Repository registration
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ILoggingRepository, LoggingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<ISignUpKeyRepository, SignUpKeyRepository>();
builder.Services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
builder.Services.AddScoped<DuplicateChecker>();

// FluentValidation (IValidator<T> for services)
builder.Services.AddValidatorsFromAssemblyContaining<SignUpRequestValidator>();

// Business services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRolesAndPermissionsService, RolesAndPermissionsService>();
builder.Services.AddScoped<ISignUpKeyService, SignUpKeyService>();
builder.Services.AddScoped<ISignUpService, SignUpService>();

// Authentication with Cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = ".AspNetCore.AuthCookie";
    // SameSite=None requires Secure; on http://localhost that conflicts with SameAsRequest (no Secure).
    // Dev: Lax + SameAsRequest — localhost ports are same-site, credentialed fetch still works.
    // Prod: None + Always — cross-origin SPA over HTTPS.
    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    }
    else
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
    }

    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;

    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";

    options.Events = new CookieAuthenticationEvents
    {
        OnValidatePrincipal = context =>
        {
            if (context.Properties.ExpiresUtc.HasValue)
            {
                var timeRemaining = context.Properties.ExpiresUtc.Value - DateTimeOffset.UtcNow;
                if (timeRemaining < TimeSpan.Zero)
                {
                    context.RejectPrincipal();
                }
            }

            return Task.CompletedTask;
        },
        OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    };
});

// Authorization with permission-based policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Permission.ReadOnly", policy =>
        policy.Requirements.Add(new PermissionRequirement(PermissionNames.ReadOnly)));

    options.AddPolicy("Permission.ReadWrite", policy =>
        policy.Requirements.Add(new PermissionRequirement(PermissionNames.ReadWrite)));

    // "all-permissions" can access everything in your model.
    options.AddPolicy("Permission.AllPermissions", policy =>
        policy.Requirements.Add(new PermissionRequirement(PermissionNames.AllPermissions)));

    // Useful for endpoints where read-write OR all-permissions should pass.
    options.AddPolicy("Permission.ReadWriteOrAll", policy =>
        policy.Requirements.Add(new PermissionRequirement(PermissionNames.ReadWrite, PermissionNames.AllPermissions)));

    // Useful for endpoints where any known permission should pass.
    options.AddPolicy("Permission.AnyKnown", policy =>
        policy.Requirements.Add(new PermissionRequirement(
            PermissionNames.ReadOnly,
            PermissionNames.ReadWrite,
            PermissionNames.AllPermissions)));
});

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// CORS: Cors:AllowedOrigins (semicolon-separated). Empty / whitespace only => use dev defaults when Development.
var corsConfig = builder.Configuration["Cors:AllowedOrigins"];
string[] allowedOrigins;
if (string.IsNullOrWhiteSpace(corsConfig))
{
    allowedOrigins = [];
}
else
{
    allowedOrigins = corsConfig
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(static o => !string.IsNullOrWhiteSpace(o))
        .Select(static o => o.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
{
    allowedOrigins =
    [
        "http://localhost:5074",
        "http://localhost",
        "http://localhost:5173",
        "http://localhost:5243",
        "https://localhost:7214",
        "https://localhost:7258",
    ];
}

if (allowedOrigins.Length == 0)
    throw new InvalidOperationException("CORS allowed origins are not set. Set Cors__AllowedOrigins (semicolon-separated) for production.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
    {
        policyBuilder.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = ".AspNetCore.Antiforgery";
    options.Cookie.HttpOnly = true;
    // SameSite=None must use Secure; HTTP dev cannot satisfy both — use Lax in Development (see auth cookie above).
    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    }
    else
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
    }
});

// AutoValidateAntiforgeryToken requires ViewFeatures (AutoValidateAntiforgeryTokenAuthorizationFilter);
// AddControllers() alone does not register it — use AddControllersWithViews().
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StockManager API",
        Version = "v1",
        Description = "REST API for StockManager (.NET 10)"
    });
});

var app = builder.Build();

// Pipeline aligned with SchoolScheduler: Swagger first; HTTPS redirect only when HTTPS_PORT is set
// (avoids redirecting plain http://localhost:5000 API calls, which breaks credentialed fetch + CORS in the browser).
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "StockManager API v1");
    c.RoutePrefix = string.Empty;
});

var httpsPort = builder.Configuration.GetValue<int?>("HTTPS_PORT");
if (httpsPort.HasValue)
    app.UseHttpsRedirection();

app.UseExceptionHandler();
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();

app.Run();
