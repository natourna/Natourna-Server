using System.Text;

namespace NatournaServer.Extensions;

public static class LoggingExtension
{
    // Requests whose bodies carry credentials and must never be written to the logs
    private static readonly string[] SensitiveBodyPaths = ["/api/Auth", "/api/User", "/api/Organization"];

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
}
