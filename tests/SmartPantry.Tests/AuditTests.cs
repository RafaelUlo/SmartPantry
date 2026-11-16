using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SmartPantry.Application.Profiles;
using SmartPantry.Domain;
using SmartPantry.Domain.AggregateRoots.Profiles;
using SmartPantry.Infrastructure.SqlLite;
using SmartPantry.Infrastructure.SqlLite.Configuration.BaseConfigurations;
using Xunit;

namespace SmartPantry.Tests;

public sealed class AuditTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "SmartPantryAuditTests", Guid.NewGuid().ToString());
    private readonly ServiceProvider provider;
    public AuditTests()
    {
        Directory.CreateDirectory(directory);
        provider = new ServiceCollection().AddSmartPantrySqlite(Path.Combine(directory, "profiles.db")).BuildServiceProvider();
    }
    private IProfileService Service => provider.GetRequiredService<IProfileService>();
    private Task<SmartPantryDbContext> Context() => provider.GetRequiredService<IDbContextFactory<SmartPantryDbContext>>().CreateDbContextAsync();
    private static HouseholdProfile Profile() => CreateProfile(Guid.NewGuid(), "Family", 4, "Vegetarian", ["Eggs"], ["Mushrooms"]);

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)] [InlineData(true, false)] [InlineData(true, true)]
    public async Task All_save_overloads_stamp_creation_update_and_soft_delete(bool asynchronous, bool accept)
    {
        await Service.LoadAsync();
        await using var db = await Context();
        var row = CreateProfile(Guid.NewGuid(), "Family", 4, "Vegan", [], []);
        db.HouseholdProfiles.Add(row);
        async Task Save()
        {
            if (asynchronous) await db.SaveChangesAsync(accept);
            else db.SaveChanges(accept);
            if (!accept) db.ChangeTracker.AcceptAllChanges();
        }
        await Save();
        var created = row.CreatedAt;
        Assert.Equal(DateTimeKind.Utc, created.Kind);
        Assert.NotEqual(default, created);
        Assert.Null(row.UpdatedAt); Assert.Null(row.DeletedAt);
        Assert.Equal("", row.CreatedBy); Assert.Null(row.UpdatedBy); Assert.Null(row.DeletedBy);
        db.Entry(row).Property(p => p.Name).CurrentValue = "Updated";
        await Save();
        Assert.Equal(created, row.CreatedAt);
        Assert.True(row.UpdatedAt >= created);
        db.HouseholdProfiles.Remove(row);
        await Save();
        Assert.NotNull(row.DeletedAt);
        Assert.False(await db.HouseholdProfiles.AnyAsync());
        var stored = await db.HouseholdProfiles.IgnoreQueryFilters().SingleAsync();
        Assert.Equal("Updated", stored.Name);
        Assert.Equal(created, stored.CreatedAt);
        Assert.Null(stored.DeletedBy);
    }

    [Fact]
    public async Task Removed_preferences_can_be_added_again_without_duplicate_keys()
    {
        var profile = Profile();
        await Service.SaveAsync(profile);
        await Service.SaveAsync(CreateProfile(profile.Id, "Family", 4, "Vegetarian", [], []));
        var empty = (await Service.LoadAsync()).Active;
        Assert.Empty(empty.Allergies); Assert.Empty(empty.Exclusions);
        var book = await Service.SaveAsync(profile);
        Assert.Single(book.Active.Allergies); Assert.Single(book.Active.Exclusions);
        Assert.NotEqual(default, book.Active.CreatedAt);
        Assert.NotEqual(default, book.Active.Allergies.Single().CreatedAt);
        await using var db = await Context();
        Assert.Equal(7, await db.Ingredients.CountAsync());
    }

    [Fact]
    public async Task Children_of_an_unloaded_deleted_profile_are_hidden_and_deleted_profile_cannot_be_selected()
    {
        var profile = Profile(); await Service.SaveAsync(profile);
        await using (var db = await Context())
        {
            db.Remove(await db.HouseholdProfiles.SingleAsync()); // children deliberately not loaded
            await db.SaveChangesAsync();
        }
        await using (var db = await Context())
        {
            Assert.Empty(await db.HouseholdProfiles.Include(p => p.Allergies).ToArrayAsync());
            // Ingredients are shared catalogue entries, not children owned by a profile.
            Assert.Equal(7, await db.Ingredients.CountAsync());
        }
        await Assert.ThrowsAsync<SmartPantry.Domain.Exceptions.DomainException>(() => Service.SetActiveAsync(profile.Id));
        Assert.Empty((await Service.LoadAsync()).Profiles);
    }

    [Fact]
    public async Task Every_table_has_the_audit_filter_lengths_and_leading_columns()
    {
        await Service.LoadAsync();
        await using var db = await Context();
        foreach (var entity in db.Model.GetEntityTypes())
        {
            if (!typeof(IAuditableEntity).IsAssignableFrom(entity.ClrType)) continue; // Association tables have no business/audit fields.
            Assert.NotEmpty(entity.GetDeclaredQueryFilters());
            foreach (var name in new[] { "CreatedBy", "UpdatedBy", "DeletedBy" })
                Assert.Equal(BaseConfiguration<HouseholdProfile>.ActorMaxLength, entity.FindProperty(name)!.GetMaxLength());
            await db.Database.OpenConnectionAsync();
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"PRAGMA table_info(\"{entity.GetTableName()}\")";
            await using var reader = await command.ExecuteReaderAsync();
            var names = new List<string>();
            while (await reader.ReadAsync()) names.Add(reader.GetString(1));
            Assert.Equal(new[] { "Id", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "DeletedAt", "DeletedBy" }, names.Take(7));
        }
    }

    [Fact]
    public async Task Upgrade_preserves_existing_profiles_preferences_and_active_selection()
    {
        var profileId = Guid.NewGuid();
        await using (var db = await Context())
        {
            var migrator = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
            await migrator.MigrateAsync("20261114110503_InitialProfiles");
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Profiles (Id, Name, Servings, Diet) VALUES ({profileId}, {"Family"}, {4}, {"Vegetarian"})");
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO AppState (Id, ActiveProfileId, LegacyImportCompleted) VALUES ({1}, {profileId}, {true})");
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO ProfileAllergies (ProfileId, Value) VALUES ({profileId}, {"Eggs"})");
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO ProfileExclusions (ProfileId, Value, Position) VALUES ({profileId}, {"Mushrooms"}, {0})");
        }
        var book = await Service.LoadAsync();
        Assert.Equal(profileId, book.ActiveId);
        Assert.Equal("Family", book.Active.Name);
        Assert.Equal("Eggs", book.Active.Allergies.Single().Name);
        Assert.Equal("Mushrooms", book.Active.Exclusions.Single().Name);
        // Rows from before auditing have unknown creation times; later saves stamp updates.
        await Service.SaveAsync(CreateProfile(profileId, "Updated", 4, "Vegetarian", [], []));
        await using var upgraded = await Context();
        Assert.NotNull((await upgraded.HouseholdProfiles.SingleAsync()).UpdatedAt);
        Assert.Equal(7, await upgraded.Ingredients.CountAsync());
    }

    private static HouseholdProfile CreateProfile(Guid id, string name, int servings, string diet, string[] allergies, string[] exclusions)
        => new(id, name, servings, diet,
            allergies.Select(value => new SmartPantry.Domain.AggregateRoots.Ingredients.Ingredient(value)),
            exclusions.Select(value => new SmartPantry.Domain.AggregateRoots.Ingredients.Ingredient(value)));

    public void Dispose()
    {
        provider.Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        Directory.Delete(directory, true);
    }
}
