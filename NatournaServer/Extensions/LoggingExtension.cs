using NatournaServer.Constants.Error;
using NatournaServer.Exceptions;
using System.Diagnostics;

namespace NatournaServer.Extensions;

public static class LoggingExtension
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLogger");
            var stopwatch = Stopwatch.StartNew();

            await next();

            stopwatch.Stop();

            logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        });

        return app;
    }

    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
            try
            {
                await next();
            }
            catch (ApiException ex)
            {
                logger.LogWarning(ex, "[{ErrorCode}] {Details}", ex.ErrorCode, ex.GetFullDetails());
                await WriteErrorAsync(context, MapStatusCode(ex.ErrorCode), ex.ErrorCode, ex.UserMessage);
            }
            catch (ContextException ex)
            {
                logger.LogError(ex, "[{ErrorCode}] {Details}", ex.ErrorCode, ex.GetFullDetails());
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, ex.ErrorCode, "An unexpected error occurred. Please try again.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred while processing request");
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "SERVER-500", "An unexpected error occurred. Please try again.");
            }
        });
        return app;
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string errorCode, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = System.Text.Json.JsonSerializer.Serialize(new { errorCode, message });
        await context.Response.WriteAsync(json);
    }

    private static int MapStatusCode(string errorCode)
    {
        return errorCode switch
        {
            ErrorCodes.BILL_NOT_FOUND_ERROR
                or ErrorCodes.PAYMENT_NOT_FOUND_ERROR
                or ErrorCodes.BALANCE_NOT_FOUND_ERROR => StatusCodes.Status404NotFound,
            ErrorCodes.BILL_ALREADY_PAID_ERROR
                or ErrorCodes.BILL_ALREADY_UNPAID_ERROR
                or ErrorCodes.PAYMENT_ALREADY_PAID_ERROR
                or ErrorCodes.PAYMENT_ALREADY_UNPAID_ERROR
                or ErrorCodes.USER_EMAIL_EXISTS_ERROR => StatusCodes.Status409Conflict,
            ErrorCodes.BILL_INSUFFICIENT_BALANCE_ERROR
                or ErrorCodes.PAYMENT_INVALID_ALLOCATIONS_ERROR => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
