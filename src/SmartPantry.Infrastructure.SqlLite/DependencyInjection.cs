using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartPantry.Application.Persistence;
using SmartPantry.Application.Profiles;

namespace SmartPantry.Infrastructure.SqlLite;

public static class DependencyInjection
{
    public static IServiceCollection AddSmartPantrySqlite(this IServiceCollection services, string databasePath, string? legacyJsonPath = null)
    {
        var connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath, ForeignKeys = true }.ToString();
        services.AddDbContextFactory<SmartPantryDbContext>(options => options.UseSqlite(connectionString));
        services.AddSingleton<ISmartPantryDbContextFactory>(provider => new SmartPantryDbContextFactory(
            provider.GetRequiredService<IDbContextFactory<SmartPantryDbContext>>(), legacyJsonPath));
        services.AddTransient<IProfileService, ProfileService>();
        services.AddSingleton<SmartPantry.Application.Catalogue.ICatalogueService, SmartPantry.Application.Catalogue.CatalogueService>();
        return services;
    }
}


