namespace NatournaServer.Extensions;

public static class SecurityExtension
{
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
}
