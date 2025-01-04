using Microsoft.EntityFrameworkCore;
using UrlShortener.Context;

namespace UrlShortener.Extensions;

public static class MigrationExtension
{
    public static void ApplyMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        dbContext.Database.Migrate();
    }
}
