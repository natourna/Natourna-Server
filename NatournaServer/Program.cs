using NatournaServer.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Host.AddSeriLog();
    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddPostgreSqlService(builder.Configuration);
    builder.Services.AddApiManagers();
    builder.Services.AddContextManagers();
    builder.Services.AddSwaggerServices();
    builder.Services.AddAuthenticationService(builder.Configuration);
    builder.Services.AddCorsPolicy(builder.Configuration);
    builder.Services.AddRateLimiting();
    builder.WebHost.AddListenPort(builder.Configuration);

    var app = builder.Build();

    await app.Services.AddContextService(builder.Configuration);

    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };
    forwardedHeadersOptions.KnownIPNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeadersOptions);

    app.UseGlobalExceptionHandling();
    app.UseSecurityHeaders();
    app.UseRequestLogging();
    app.UseSwaggerServices();

    app.UseCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    string errorMsg = $"Startup failed : {ex.Message}";
    Console.WriteLine(errorMsg);

    Log.Error(ex, "{ErrorMessage}", errorMsg);
}
