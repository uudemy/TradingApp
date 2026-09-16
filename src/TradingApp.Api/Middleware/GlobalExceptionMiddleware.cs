using System.Net;
using System.Text.Json;
using TradingApp.Api.Common;
using Microsoft.EntityFrameworkCore;
namespace TradingApp.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception. Method={Method} Path={Path} TraceId={TraceId}",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);

            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        
        {
            DbUpdateException dbEx => (
    (int)HttpStatusCode.Conflict,
    _env.IsDevelopment()
        ? (dbEx.InnerException?.Message ?? dbEx.Message)
        : "A database error occurred.",
    new[] { "db_update_failed" }),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized", Array.Empty<string>()),
            KeyNotFoundException nfe => ((int)HttpStatusCode.NotFound, nfe.Message, Array.Empty<string>()),
            ArgumentException ae => ((int)HttpStatusCode.BadRequest, ae.Message, Array.Empty<string>()),
            _ => ((int)HttpStatusCode.InternalServerError,
                  _env.IsDevelopment() ? exception.Message : "Beklenmeyen bir hata oluştu.",
                  Array.Empty<string>())
            
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var payload = ApiResponse<object>.Fail(message, errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}