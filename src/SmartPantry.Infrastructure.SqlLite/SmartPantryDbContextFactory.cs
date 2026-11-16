using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartPantry.Application.Persistence;
using SmartPantry.Domain.AggregateRoots.Profiles;
using SmartPantry.Domain.Entities;
using SmartPantry.Domain.AggregateRoots.Ingredients;

namespace SmartPantry.Infrastructure.SqlLite;

internal sealed class SmartPantryDbContextFactory(IDbContextFactory<SmartPantryDbContext> factory, string? legacyJsonPath)
    : ISmartPantryDbContextFactory
{
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private bool initialized;

    public async Task<ISmartPantryDbContext> CreateAsync(CancellationToken cancellationToken = default)
    {
        await initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (!initialized)
            {
                await using var db = await factory.CreateDbContextAsync(cancellationToken);
                await db.Database.MigrateAsync(cancellationToken);
                await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
                var state = await db.LegacyImportState.SingleOrDefaultAsync(cancellationToken) ?? new LegacyImportState();
                if (db.Entry(state).State == EntityState.Detached) db.LegacyImportState.Add(state);
                if (!state.LegacyImportCompleted)
                {
                    if (!await db.HouseholdProfiles.IgnoreQueryFilters().AnyAsync(cancellationToken) && legacyJsonPath is not null && File.Exists(legacyJsonPath))
                    {
                        var json = await File.ReadAllTextAsync(legacyJsonPath, cancellationToken);
                        var legacy = JsonSerializer.Deserialize<LegacyBook>(json) ?? throw new InvalidDataException("Invalid legacy profile data.");
                        var profiles = legacy.Profiles.Select(p => new HouseholdProfile(p.Id, p.Name, p.Servings, p.Diet, p.Allergies.Select(name => new Ingredient(name)), p.Exclusions.Select(name => new Ingredient(name)))).ToArray();
                        if (profiles.Length > 0 && !profiles.Any(p => p.Id == legacy.ActiveId)) throw new InvalidDataException("The legacy active profile does not exist.");
                        var diets = (await db.Diets.ToListAsync(cancellationToken)).ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);
                        var ingredients = (await db.Ingredients.ToListAsync(cancellationToken)).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                        Ingredient Resolve(Ingredient ingredient)
                        {
                            if (!ingredients.TryGetValue(ingredient.Name, out var shared))
                                ingredients.Add(ingredient.Name, shared = ingredient);
                            return shared;
                        }
                        foreach (var profile in profiles)
                        {
                            if (!diets.TryGetValue(profile.Diet.Name, out var diet))
                                diets.Add(profile.Diet.Name, diet = profile.Diet);
                            var imported = new HouseholdProfile(profile.Id, profile.Name, profile.Servings, diet,
                                profile.Allergies.Select(Resolve), profile.Exclusions.Select(Resolve));
                            imported.SetDefault(profile.Id == legacy.ActiveId);
                            db.HouseholdProfiles.Add(imported);
                        }
                    }
                    state.LegacyImportCompleted = true;
                }
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                initialized = true;
            }
        }
        finally { initializationLock.Release(); }
        return await factory.CreateDbContextAsync(cancellationToken);
    }

    private sealed record LegacyBook(Guid ActiveId, LegacyProfile[] Profiles);
    private sealed record LegacyProfile(Guid Id, string Name, int Servings, string Diet, string[] Allergies, string[] Exclusions);
}
