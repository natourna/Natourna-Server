
public static class ContextServiceExtension 
{
    public static async Task AddArteContextService(this IServiceProvider services, bool isDev)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider;
        var context = service.GetRequiredService<IngestContext>();

        if (context.Database.IsOracle())
        {
            if (isDev)
            {
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                await context.Database.MigrateAsync();
            }
        }
    }

}