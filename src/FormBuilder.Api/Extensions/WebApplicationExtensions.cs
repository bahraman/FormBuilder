using FormBuilder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigration");

        try
        {
            if (dbContext.Database.IsRelational())
            {
                logger.LogInformation("Applying database migrations...");
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw;
        }
    }
}
