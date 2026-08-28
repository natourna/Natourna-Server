using NatrounaServer.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Add services using extensions
    builder.Host.AddSeriLog();
    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor(); // Required for AuditService
    builder.Services.AddPostgreSqlService(builder.Configuration);
    builder.Services.AddApiManagers();
    builder.Services.AddContextManagers();
    builder.Services.AddSwaggerServices();
    builder.Services.AddAuthenticationService(builder.Configuration);
    builder.WebHost.AddListenPort(builder.Configuration);

    var app = builder.Build();

    await app.Services.AddContextService(app.Environment.IsDevelopment());

    app.UseGlobalExceptionLogging();
    app.UseRequestLogging();
    app.UseSwaggerServices();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseCors("AllowLocalhost4200");

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    string errorMsg = $"Startup failed : {ex.Message}";
    Console.WriteLine(errorMsg);

    Log.Error(ex, "{ErrorMessage}", errorMsg);
}