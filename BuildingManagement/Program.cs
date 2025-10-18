using BuildingManagement.Data;
using BuildingManagement.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Use PORT environment variable if set (for Google Cloud Run)
var port = Environment.GetEnvironmentVariable("PORT");
var listenPort = string.IsNullOrEmpty(port) ? 8080 : int.Parse(port);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(listenPort);
});

Console.WriteLine($" App is starting on port {listenPort}...");

// Setup Serilog for file logging
var appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
var logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
Directory.CreateDirectory(logDir);
var logFile = Path.Combine(logDir, $"{appName}-{DateTime.UtcNow:yyyyMMdd}.log");

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
    .CreateLogger();

if (File.Exists("appsettings.local.json"))
{
    builder.Configuration.AddJsonFile(
        "appsettings.local.json",
        optional: true,
        reloadOnChange: true
    );
}

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

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

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<BuildingManagementContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to resolve BuildingManagementContext: " + ex.ToString());
        // Optionally, rethrow or handle as needed
    }
}

// Configure the HTTP request pipeline.
app.UseGlobalExceptionLogging();
app.UseRequestLogging();
app.UseSwaggerServices();

app.UseAuthentication(); // Add this before UseAuthorization
app.UseAuthorization();

app.UseCors("AllowLocalhost4200");

app.MapControllers();

app.Run();
