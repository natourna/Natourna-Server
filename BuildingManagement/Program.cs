using BuildingManagement.Extensions;
using BuildingManagement.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Setup Serilog for file logging
var appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
var logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
Directory.CreateDirectory(logDir);
var logFile = Path.Combine(logDir, $"{appName}-{DateTime.UtcNow:yyyyMMdd}.log");

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
    .CreateLogger();

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
