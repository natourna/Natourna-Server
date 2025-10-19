using System.Text;
using BuildingManagement.Authentication;
using BuildingManagement.Interfaces.Authentication;
using BuildingManagement.Models.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BuildingManagement.Extensions
{
    public static class AuthExtension
    {
        public static void AddAuthenticationService(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure JWT Settings
            services.Configure<JwtConfiguration>(configuration.GetSection("JwtSettings"));

            // Register JWT Service
            services.AddScoped<IJwtAuthenticationService, JwtAuthenticationService>();

            // Get JWT settings for authentication configuration
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtConfiguration>();

            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JWT settings are not properly configured");
            }

            var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}
