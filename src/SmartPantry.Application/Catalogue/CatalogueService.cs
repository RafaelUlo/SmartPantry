using Microsoft.EntityFrameworkCore;
using SmartPantry.Application.Persistence;
using SmartPantry.Domain.AggregateRoots.Ingredients;

namespace SmartPantry.Application.Catalogue;

public sealed class CatalogueService(ISmartPantryDbContextFactory factory) : ICatalogueService
{
    private readonly SemaphoreSlim creationLock = new(1, 1);

    public async Task<ProfileOptions> GetProfileOptionsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateAsync(cancellationToken);
        var diets = await db.Diets.AsNoTracking().OrderBy(d => d.Name).Select(d => new CatalogueOption(d.Id, d.Name)).ToArrayAsync(cancellationToken);
        var allergens = await db.Ingredients.AsNoTracking().OrderBy(i => i.Name)
            .Select(i => new CatalogueOption(i.Id, i.Name)).ToArrayAsync(cancellationToken);
        return new(diets, allergens);
    }

    public async Task<CatalogueOption[]> SearchIngredientsAsync(string search, CancellationToken cancellationToken = default)
    {
        var value = search.Trim();
        if (value.Length == 0) return [];
        var pattern = "%" + value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
        await using var db = await factory.CreateAsync(cancellationToken);
        return await db.Ingredients.AsNoTracking().Where(i => EF.Functions.Like(i.Name, pattern, "\\"))
            .OrderBy(i => i.Name).Take(20).Select(i => new CatalogueOption(i.Id, i.Name)).ToArrayAsync(cancellationToken);
    }

    public async Task<CatalogueOption> CreateIngredientAsync(string name, CancellationToken cancellationToken = default)
    {
        var value = name.Trim();
        if (value.Length == 0) throw new ArgumentException("Enter an ingredient name.", nameof(name));
        await creationLock.WaitAsync(cancellationToken);
        try
        {
            await using var db = await factory.CreateAsync(cancellationToken);
            var existing = await db.Ingredients.FirstOrDefaultAsync(i => EF.Functions.Collate(i.Name, "NOCASE") == value, cancellationToken);
            if (existing is not null) return new(existing.Id, existing.Name);
            var ingredient = new Ingredient(value);
            db.Ingredients.Add(ingredient);
            await db.SaveChangesAsync(cancellationToken);
            return new(ingredient.Id, ingredient.Name);
        }
        finally { creationLock.Release(); }
    }
}
