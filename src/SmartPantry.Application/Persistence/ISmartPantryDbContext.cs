using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartPantry.Domain.AggregateRoots.Ingredients;
using SmartPantry.Domain.AggregateRoots.Profiles;
using SmartPantry.Domain.Entities;

namespace SmartPantry.Application.Persistence;

public interface ISmartPantryDbContext : IAsyncDisposable
{
    DbSet<HouseholdProfile> HouseholdProfiles { get; }
    DbSet<Ingredient> Ingredients { get; }
    DbSet<Diet> Diets { get; }
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// A fresh unit of work per operation avoids sharing EF tracking between Blazor events.
public interface ISmartPantryDbContextFactory
{
    Task<ISmartPantryDbContext> CreateAsync(CancellationToken cancellationToken = default);
}
