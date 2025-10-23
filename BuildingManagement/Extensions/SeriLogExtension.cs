using Serilog;

namespace BuildingManagement.Extensions
{
    public static class SeriLogExtension
    {
        public static void AddSeriLog(this IHostBuilder builder)
        {
            var appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            // Use /app/logs for Docker container (mounted volume)
            // Falls back to local Logs directory for local development
            var logDir = Path.Combine("/app/logs");

            // For local development (non-Docker), use local Logs folder
            if (!Directory.Exists("/app"))
            {
                logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
            }

            Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, $"{appName}-{DateTime.UtcNow:yyyyMMdd}.log");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console() // Also log to console for docker logs
                .WriteTo.File(
                    path: logFile,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30, // Keep 30 days of logs
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            SerilogHostBuilderExtensions.UseSerilog(builder);
        }
    }
}
