using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartPantry.Infrastructure.SqlLite;

public sealed class SmartPantryDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SmartPantryDbContext>
{
    public SmartPantryDbContext CreateDbContext(string[] args)
    {
        // Tooling uses a separate path, never the installed mobile application's database.
        var path = args.FirstOrDefault() ?? Path.Combine(Environment.CurrentDirectory, "smartpantry.design.db");
        var connection = new SqliteConnectionStringBuilder { DataSource = path, ForeignKeys = true }.ToString();
        return new(new DbContextOptionsBuilder<SmartPantryDbContext>().UseSqlite(connection).Options);
    }
}
