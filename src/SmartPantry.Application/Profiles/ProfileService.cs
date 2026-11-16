using Microsoft.EntityFrameworkCore;
using SmartPantry.Application.Dtos;
using SmartPantry.Application.Persistence;
using SmartPantry.Domain.AggregateRoots.Ingredients;
using SmartPantry.Domain.AggregateRoots.Profiles;
using SmartPantry.Domain.Entities;
using SmartPantry.Domain.Exceptions;

namespace SmartPantry.Application.Profiles;

public sealed class ProfileService(ISmartPantryDbContextFactory factory) : IProfileService
{
    public async Task<HouseholdProfileDto> LoadAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateAsync(cancellationToken);
        return await LoadAsync(db, cancellationToken);
    }

    public async Task<HouseholdProfileDto> SaveAsync(HouseholdProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        await using var db = await factory.CreateAsync(cancellationToken);

        var existing = await db.HouseholdProfiles
            .Include(p => p.Allergies)
            .Include(p => p.Exclusions)
            .SingleOrDefaultAsync(p => p.Id == profile.Id, cancellationToken);

        var diet = await db.Diets
            .SingleOrDefaultAsync(d => d.Name == profile.Diet.Name, cancellationToken);

        if (diet is null)
        {
            diet = new Diet(profile.Diet.Name); db.Diets.Add(diet);
        }

        var catalogue = await db.Ingredients.ToListAsync(cancellationToken);

        Ingredient Resolve(Ingredient input)
        {
            var ingredient = catalogue.FirstOrDefault(i => string.Equals(i.Name, input.Name, StringComparison.OrdinalIgnoreCase));

            if (ingredient is not null)
            {
                return ingredient;
            }

            ingredient = new Ingredient(input.Name);

            catalogue.Add(ingredient);

            db.Ingredients.Add(ingredient);

            return ingredient;
        }

        var allergies = profile.Allergies
            .Select(Resolve)
            .ToList();

        var exclusions = profile.Exclusions
            .Select(Resolve)
            .ToList();

        if (existing is null)
        {
            existing = new HouseholdProfile(profile.Id, profile.Name, profile.Servings, diet, allergies, exclusions);

            db.HouseholdProfiles.Add(existing);
        }
        else
        {
            existing.Update(profile.Name, profile.Servings, diet, allergies, exclusions);
        }

        if (!await db.HouseholdProfiles.AnyAsync(p => p.IsDefault, cancellationToken))
        {
            existing.SetDefault(true);
        }

        await db.SaveChangesAsync(cancellationToken);

        return await LoadAsync(db, cancellationToken);
    }

    public async Task<HouseholdProfileDto> SetActiveAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateAsync(cancellationToken);

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        var selected = await db.HouseholdProfiles.SingleOrDefaultAsync(p => p.Id == profileId, cancellationToken)
            ?? throw new DomainException("The selected profile does not exist.");

        var previous = await db.HouseholdProfiles.SingleOrDefaultAsync(p => p.IsDefault, cancellationToken);

        if (previous is not null && previous.Id != selected.Id)
        {
            previous.SetDefault(false);

            await db.SaveChangesAsync(cancellationToken);
        }

        selected.SetDefault(true);

        await db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return await LoadAsync(db, cancellationToken);
    }

    private static async Task<HouseholdProfileDto> LoadAsync(ISmartPantryDbContext db, CancellationToken cancellationToken)
    {
        var profiles = await db.HouseholdProfiles
            .AsNoTracking()
            .Include(p => p.Diet)
            .Include(p => p.Allergies)
            .Include(p => p.Exclusions)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        if (profiles.Count > 0 && !profiles.Any(p => p.IsDefault))
        {
            var fallback = await db.HouseholdProfiles.SingleAsync(p => p.Id == profiles[0].Id, cancellationToken);

            fallback.SetDefault(true);

            await db.SaveChangesAsync(cancellationToken);

            profiles[0].SetDefault(true);
        }
        return new(profiles.SingleOrDefault(p => p.IsDefault)?.Id ?? Guid.Empty, profiles);
    }
}
