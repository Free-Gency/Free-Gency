using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FreeGency.Infrastructure.Persistence.Seeding;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<ApplicationDbContext>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

        await context.Database.MigrateAsync(ct);
        logger.LogInformation("Database migrations applied.");

        await TaxonomySeeder.SeedAsync(context, ct);
        logger.LogInformation("Taxonomy seed completed.");
    }
}
