using System.Text.Json;
using FluentValidation;
using Logistics.Shared.Models;
using Serilog;

namespace Logistics.API.Middlewares;

/// <summary>
/// Middleware that handles exceptions and captures error response bodies for logging.
/// Converts exceptions to appropriate HTTP responses and enriches Serilog request logs with error details.
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public const string ErrorBodyKey = "ErrorResponseBody";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);

            if (GetStatusCode(ex) == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(ex, "Unhandled exception occurred");
            }
        }
        finally
        {
            // Capture error response body for 4xx and 5xx status codes
            if (context.Response.StatusCode >= 400)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var body = await new StreamReader(responseBody).ReadToEndAsync();

                if (!string.IsNullOrWhiteSpace(body))
                {
                    context.Items[ErrorBodyKey] = body;
                }
            }

            // Copy the response body back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }

    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var response = CreateErrorResponse(exception);

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static ErrorResponse CreateErrorResponse(Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            var details = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return new ErrorResponse("Validation failed", details);
        }

        return new ErrorResponse(exception.Message);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    /// <summary>
    /// Configures Serilog request logging with error response details enrichment.
    /// </summary>
    public static IApplicationBuilder UseSerilogRequestLoggingWithErrorDetails(this IApplicationBuilder builder)
    {
        return builder.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex is not null)
                {
                    return Serilog.Events.LogEventLevel.Error;
                }

                return httpContext.Response.StatusCode switch
                {
                    >= 500 => Serilog.Events.LogEventLevel.Error,
                    >= 400 => Serilog.Events.LogEventLevel.Warning,
                    _ => Serilog.Events.LogEventLevel.Information
                };
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId",
                        httpContext.User.FindFirst("sub")?.Value ??
                        httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                }

                if (httpContext.Items.TryGetValue(ExceptionHandlingMiddleware.ErrorBodyKey, out var errorBody)
                    && errorBody is string body)
                {
                    const int maxLength = 4000;
                    diagnosticContext.Set("ErrorBody",
                        body.Length > maxLength ? body[..maxLength] + "... (truncated)" : body);
                }

                if (httpContext.Response.StatusCode >= 400 && httpContext.Request.QueryString.HasValue)
                {
                    diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value);
                }
            };
        });
    }
}
