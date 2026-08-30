using Microsoft.AspNetCore.HttpOverrides;

namespace NatournaServer.Extensions;

public static class SecurityExtension
{
    /// <summary>Honors X-Forwarded-For/Proto from the reverse proxy so audit IPs see the real client.</summary>
    public static IApplicationBuilder UseProxyForwardedHeaders(this IApplicationBuilder app)
    {
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        // The app is only reachable through the compose network's proxy (Caddy),
        // whose address is dynamic - so the default loopback-only trust list is cleared.
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();

        return app.UseForwardedHeaders(options);
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        string[] origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (origins.Length > 0)
                {
                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            await next();
        });

        return app;
    }
}
