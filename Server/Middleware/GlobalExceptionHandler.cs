using System.Net;
using System.Net.Mime;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Server.Models.Logging;
using Server.Repositories.Interfaces;
using Shared.Dtos;

namespace Server.Middleware;

/// <summary>
/// Maps unhandled exceptions to <see cref="ApiError"/> JSON, logs with <see cref="ILogger"/>,
/// and persists unexpected failures to <see cref="ILoggingRepository"/>.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        var path = httpContext.Request.Path.Value ?? string.Empty;

        int statusCode;
        ApiError body;

        switch (exception)
        {
            case ValidationException vex:
                (statusCode, body) = HandleValidation(vex, traceId, path);
                break;
            case ArgumentException arg:
                (statusCode, body) = HandleArgument(arg, traceId, path);
                break;
            case InvalidOperationException inv:
                (statusCode, body) = HandleConflict(inv, traceId, path);
                break;
            default:
                (statusCode, body) = await HandleUnexpectedAsync(exception, traceId, path, cancellationToken);
                break;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        await httpContext.Response.WriteAsJsonAsync(body, cancellationToken);
        return true;
    }

    private (int, ApiError) HandleValidation(ValidationException vex, string traceId, string path)
    {
        var dict = vex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        _logger.LogWarning(
            "Validation failed for {Path}. TraceId: {TraceId}. Details are not logged to the console.",
            path,
            traceId);
        return ((int)HttpStatusCode.BadRequest, ApiError.ValidationError(dict, traceId));
    }

    private (int, ApiError) HandleArgument(ArgumentException arg, string traceId, string path)
    {
        _logger.LogWarning(
            "Not found or bad argument for {Path}. TraceId: {TraceId}. Details are not logged to the console.",
            path,
            traceId);
        return ((int)HttpStatusCode.NotFound, ApiError.NotFound(arg.Message, traceId));
    }

    private (int, ApiError) HandleConflict(InvalidOperationException inv, string traceId, string path)
    {
        _logger.LogWarning(
            "Conflict for {Path}. TraceId: {TraceId}. Details are not logged to the console.",
            path,
            traceId);
        return ((int)HttpStatusCode.Conflict, new ApiError(inv.Message, "CONFLICT", traceId));
    }

    private async Task<(int StatusCode, ApiError Body)> HandleUnexpectedAsync(
        Exception exception,
        string traceId,
        string path,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            "An unexpected error occurred for {Path}. TraceId: {TraceId}. Full details were written to ErrorLogs.",
            path,
            traceId);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var loggingRepository = scope.ServiceProvider.GetRequiredService<ILoggingRepository>();
            await loggingRepository.AddAsync(new ErrorLogging
            {
                Message = $"[{traceId}] {exception.Message}",
                StackTrace = exception.ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = nameof(GlobalExceptionHandler)
            });
        }
        catch (Exception)
        {
            _logger.LogCritical(
                "Failed to persist error to ErrorLogs for {Path}. TraceId: {TraceId}",
                path,
                traceId);
        }

        var publicMessage = _environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred.";

        return ((int)HttpStatusCode.InternalServerError, ApiError.InternalServerError(publicMessage, traceId));
    }
}
