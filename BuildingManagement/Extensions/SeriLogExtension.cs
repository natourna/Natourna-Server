using Serilog;

namespace BuildingManagement.Extensions
{
    public static class SeriLogExtension
    {
        public static void AddSeriLog(this IHostBuilder builder)
        {
            var appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            var logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
            Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, $"{appName}-{DateTime.UtcNow:yyyyMMdd}.log");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            SerilogHostBuilderExtensions.UseSerilog(builder);
        }
    }
}
