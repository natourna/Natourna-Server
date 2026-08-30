using NatournaServer.Authentication;
using NatournaServer.Interfaces.Authentication;
using NatournaServer.Models.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace NatournaServer.Extensions
{
    public static class AuthExtension
    {
        public static void AddAuthenticationService(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure JWT Settings
            services.Configure<JwtConfiguration>(configuration.GetSection("JwtSettings"));

            // Register JWT Service
            services.AddScoped<IJwtAuthenticationService, JwtAuthenticationService>();

            // Register Password Hashing Service
            services.AddSingleton<IPasswordHashingService, PasswordHashingService>();

            // Get JWT settings for authentication configuration
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtConfiguration>();

            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JWT settings are not properly configured");
            }

            // HS256 needs a 256-bit key; anything shorter fails at token generation with a cryptic error
            if (jwtSettings.SecretKey.Length < 32)
            {
                throw new InvalidOperationException("JwtSettings:SecretKey must be at least 32 characters");
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
