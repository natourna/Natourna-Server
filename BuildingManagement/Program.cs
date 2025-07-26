using BuildingManagement.Data;
using BuildingManagement.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

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
    var dbContext = scope.ServiceProvider.GetRequiredService<BuildingManagementContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseGlobalExceptionLogging();
app.UseRequestLogging();
app.UseSwaggerServices();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
