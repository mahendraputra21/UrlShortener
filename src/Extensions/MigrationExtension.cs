using Microsoft.EntityFrameworkCore;
using UrlShortener.Context;

namespace UrlShortener.Extensions;

/// <summary>
/// Extension methods for applying database migrations.
/// </summary>
public static class MigrationExtension
{
    /// <summary>
    /// Applies pending migrations to the database.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    public static void ApplyMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("MigrationExtension");

        try
        {
            var dbContext = services.GetRequiredService<ApplicationDBContext>();
            dbContext.Database.Migrate();
            logger.LogInformation("Database migration applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying the database migration.");
            throw;
        }
    }
}
