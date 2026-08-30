using NatournaServer.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Add services using extensions
    builder.Host.AddSeriLog();
    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor(); // Required for AuditService and HttpTenantContext
    builder.Services.AddTenancy(); // Must precede the DbContext, which consumes ITenantContext
    builder.Services.AddPostgreSqlService(builder.Configuration);
    builder.Services.AddApiManagers();
    builder.Services.AddContextManagers();
    builder.Services.AddSwaggerServices();
    builder.Services.AddAuthenticationService(builder.Configuration);
    builder.Services.AddCorsPolicy(builder.Configuration);
    builder.WebHost.AddListenPort(builder.Configuration);

    var app = builder.Build();

    await app.Services.AddContextService(app.Environment.IsDevelopment());
    await app.Services.SeedRolesAsync();

    app.UseProxyForwardedHeaders();
    app.UseExceptionHandling();
    app.UseSecurityHeaders();
    app.UseRequestLogging();
    app.UseSwaggerServices();

    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
}
catch (HostAbortedException)
{
    // Thrown by EF Core design-time tools (dotnet ef) after capturing the service provider - not a failure
    throw;
}
catch (Exception ex)
{
    string errorMsg = $"Startup failed : {ex.Message}";
    Console.WriteLine(errorMsg);

    Log.Error(ex, "{ErrorMessage}", errorMsg);

    // Non-zero exit so the container orchestrator restarts a failed boot instead of reporting success
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}