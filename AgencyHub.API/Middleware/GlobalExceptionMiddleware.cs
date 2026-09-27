using System.Net;
using System.Text.Json;

namespace AgencyHub.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger
        <GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;

        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(
                ex,
                "Unauthorized request.");

            await WriteErrorResponse(
                context,
                HttpStatusCode.Unauthorized,
                "Unauthorized request.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Invalid request.");

            await WriteErrorResponse(
                context,
                HttpStatusCode.BadRequest,
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "Invalid operation.");

            await WriteErrorResponse(
                context,
                HttpStatusCode.BadRequest,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled application exception.");

            await WriteErrorResponse(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.StatusCode =
            (int)statusCode;

        context.Response.ContentType =
            "application/json";

        var response = new
        {
            statusCode =
                (int)statusCode,

            message,

            timestamp =
                DateTime.UtcNow
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}