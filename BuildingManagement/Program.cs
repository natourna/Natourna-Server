using BuildingManagement.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Add services using extensions
    builder.Host.AddSeriLog();
    builder.Services.AddControllers();
    builder.Services.AddMySqlService(builder.Configuration);
    builder.Services.AddApiManagers();
    builder.Services.AddContextManagers();
    builder.Services.AddSwaggerServices();
    builder.Services.AddAuthenticationService();
    builder.WebHost.AddListenPort(builder.Configuration);

// Add Basic Authentication
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

// Add services using extensions
builder.Services
    .AddDatabaseServices(builder.Configuration)
    .AddApiManagers()
    .AddContextManagers()
    .AddSwaggerServices();

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