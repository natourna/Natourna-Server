using System.Text;

namespace NatournaServer.Extensions;

public static class LoggingExtension
{
    private static readonly string[] SensitiveBodyPaths = ["/api/Auth", "/api/User"];

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLogger");

            context.Request.EnableBuffering();

            string body = string.Empty;

            var isSensitivePath = SensitiveBodyPaths.Any(path => context.Request.Path.StartsWithSegments(path));

            if (context.Request.ContentLength > 0 && context.Request.Body.CanRead && !isSensitivePath)
            {
                context.Request.Body.Position = 0;
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            logger.LogInformation("HTTP {Method} {Path} Body: {Body}", context.Request.Method, context.Request.Path, body);

            await next();
        });

        return app;
    }

    public static IApplicationBuilder UseGlobalExceptionLogging(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionLogger");
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred while processing request");
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
#if DEBUG
                    stackTrace = ex.StackTrace
#endif
                };

                var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(json);
            }
        });
        return app;
    }
}
