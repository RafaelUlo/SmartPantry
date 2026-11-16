using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartPantry.Application.Catalogue;
using SmartPantry.Infrastructure.SqlLite;
using SmartPantry.Domain.Entities;
using Xunit;

namespace SmartPantry.Tests;

public sealed class CatalogueTests
{
    [Fact]
    public async Task Options_and_search_read_database_and_creation_reuses_names()
    {
        var path = Path.Combine(Path.GetTempPath(), $"SmartPantryCatalogue-{Guid.NewGuid()}.db");
        try
        {
            await using var provider = new ServiceCollection().AddSmartPantrySqlite(path).BuildServiceProvider();
            var catalogue = provider.GetRequiredService<ICatalogueService>();
            Assert.Equal(5, (await catalogue.GetProfileOptionsAsync()).Diets.Length);
            Assert.Empty(await catalogue.SearchIngredientsAsync("unmatched vegetable"));
            var created = await catalogue.CreateIngredientAsync("  Unmatched vegetable  ");
            var duplicate = await catalogue.CreateIngredientAsync("UNMATCHED VEGETABLE");
            Assert.Equal(created.Id, duplicate.Id);
            Assert.Equal(created.Id, Assert.Single(await catalogue.SearchIngredientsAsync("VEGETABLE")).Id);
            Assert.Contains((await catalogue.GetProfileOptionsAsync()).Allergens, i => i.Id == created.Id);
            await using var db = await provider.GetRequiredService<IDbContextFactory<SmartPantryDbContext>>().CreateDbContextAsync();
            db.Diets.Add(new Diet("Database option"));
            await db.SaveChangesAsync();
            Assert.Contains((await catalogue.GetProfileOptionsAsync()).Diets, d => d.Name == "Database option");
            await catalogue.CreateIngredientAsync("100% special_food");
            Assert.Single(await catalogue.SearchIngredientsAsync("% special_"));
            Assert.Empty(await catalogue.SearchIngredientsAsync(" "));
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
