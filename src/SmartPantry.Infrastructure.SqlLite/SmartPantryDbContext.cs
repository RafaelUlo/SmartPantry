using Microsoft.EntityFrameworkCore;
using SmartPantry.Domain;
using SmartPantry.Application.Persistence;
using SmartPantry.Domain.Entities;
using SmartPantry.Domain.AggregateRoots.Ingredients;
using SmartPantry.Domain.AggregateRoots.Profiles;
using SmartPantry.Infrastructure.SqlLite.Configuration;

namespace SmartPantry.Infrastructure.SqlLite;

public sealed class SmartPantryDbContext(DbContextOptions<SmartPantryDbContext> options) : DbContext(options), ISmartPantryDbContext
{
    internal DbSet<LegacyImportState> LegacyImportState => Set<LegacyImportState>();

    public Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);
    public DbSet<HouseholdProfile> HouseholdProfiles => Set<HouseholdProfile>();

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    public DbSet<Diet> Diets => Set<Diet>();

    public override int SaveChanges() => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAudit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAudit()
    {
        ChangeTracker.DetectChanges();

        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>().ToArray())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.CreatedAt).CurrentValue = now;
                entry.Property(e => e.UpdatedAt).CurrentValue = null;
                entry.Property(e => e.DeletedAt).CurrentValue = null;
            }
            else if (entry.State == EntityState.Deleted)
            {
                // Update only the tombstone, preserving stored business fields and creation audit.
                entry.State = EntityState.Unchanged;
                entry.Property(e => e.DeletedAt).CurrentValue = now;
                entry.Property(e => e.DeletedAt).IsModified = true;
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
                entry.Property(e => e.UpdatedAt).IsModified = true;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.CreatedAt).IsModified = false;
                entry.Property(e => e.CreatedBy).IsModified = false;
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(HouseholdProfileConfiguration).Assembly);
    }
}
