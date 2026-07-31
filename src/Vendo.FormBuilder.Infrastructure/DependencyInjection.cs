using Vendo.FormBuilder.Domain.Interfaces;
using Vendo.FormBuilder.Infrastructure.Persistence;
using Vendo.FormBuilder.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vendo.FormBuilder.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
            }));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFormRepository, FormRepository>();
        services.AddScoped<IFormResponseRepository, FormResponseRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();

        return services;
    }
}
