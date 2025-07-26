using System.Text;

namespace BuildingManagement.Extensions;

public static class LoggingExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLogger");
            context.Request.EnableBuffering();
            string body = string.Empty;
            if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
            {
                context.Request.Body.Position = 0;
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }
            logger.LogInformation($"HTTP {context.Request.Method} {context.Request.Path} Body: {body}");
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
                await context.Response.WriteAsync("An unexpected error occurred.");
            }
        });
        return app;
    }
}
