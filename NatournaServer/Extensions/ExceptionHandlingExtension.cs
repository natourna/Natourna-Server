using NatournaServer.Exceptions;
using NatournaServer.Models.Api.Response.Error;
using System.Text.Json;

namespace NatournaServer.Extensions;

public static class ExceptionHandlingExtension
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ExceptionHandler");

            try
            {
                await next();
            }
            catch (ApiException ex)
            {
                logger.LogWarning(ex, "[{ErrorCode}] {UserMessage} - {TechnicalDetails}", ex.ErrorCode, ex.UserMessage, ex.TechnicalDetails);
                await WriteErrorAsync(context, ex.StatusCode, ex.ErrorCode, ex.UserMessage);
            }
            catch (ContextException ex)
            {
                logger.LogError(ex, "[{ErrorCode}] {UserMessage} - {TechnicalDetails}", ex.ErrorCode, ex.UserMessage, ex.TechnicalDetails);
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, ex.ErrorCode, ex.UserMessage);
            }
            catch (CustomException ex)
            {
                logger.LogError(ex, "[{ErrorId}] {CustomMessage}", ex.ErrorId, ex.CustomMessage);
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, ex.ErrorCode, "An unexpected error occurred.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred while processing request");
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "SERVER-500", "An unexpected error occurred.");
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

        var errorResponse = new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, SerializerOptions));
    }
}
