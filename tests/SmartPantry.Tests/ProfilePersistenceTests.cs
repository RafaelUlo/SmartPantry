using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartPantry.Application.Profiles;
using SmartPantry.Domain.AggregateRoots.Profiles;
using SmartPantry.Infrastructure.SqlLite;
using SmartPantry.Mobile.Ui;
using Xunit;

namespace SmartPantry.Tests;

public sealed class ProfilePersistenceTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "SmartPantryTests", Guid.NewGuid().ToString());
    private string DatabasePath => Path.Combine(directory, "profiles.db");
    private string LegacyPath => Path.Combine(directory, "households.json");

    public ProfilePersistenceTests() => Directory.CreateDirectory(directory);
    private ServiceProvider Open() => new ServiceCollection().AddSmartPantrySqlite(DatabasePath, LegacyPath).BuildServiceProvider();
    private static HouseholdProfile Profile(string name = "Family") => CreateProfile(Guid.NewGuid(), name, 4, "Vegetarian", ["Eggs"], ["Mushrooms", "Coriander"]);

    [Fact]
    public async Task Fresh_database_applies_migrations_and_initialization_is_repeatable()
    {
        await using var provider = Open();
        var service = provider.GetRequiredService<IProfileService>();
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => service.LoadAsync()));
        Assert.Empty((await service.LoadAsync()).Profiles);
        await using var db = await provider.GetRequiredService<IDbContextFactory<SmartPantryDbContext>>().CreateDbContextAsync();
        Assert.Equal(6, (await db.Database.GetAppliedMigrationsAsync()).Count());
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal(new[] { "No specific restriction", "Vegetarian", "Vegan", "Pescatarian", "Other" }.OrderBy(n => n), (await db.Diets.Select(d => d.Name).ToArrayAsync()).OrderBy(n => n));
        Assert.Equal(new[] { "Milk / Dairy", "Eggs", "Peanuts", "Tree nuts", "Fish", "Shellfish" }.OrderBy(n => n), (await db.Ingredients.Select(i => i.Name).ToArrayAsync()).OrderBy(n => n));
    }

    [Fact]
    public async Task Profiles_restrictions_edits_and_selection_survive_reopening()
    {
        var family = Profile(); var vegan = CreateProfile(Guid.NewGuid(), "Vegan", 2, "Vegan", [], []);
        await using (var provider = Open())
        {
            var service = provider.GetRequiredService<IProfileService>();
            Assert.Equal(family.Id, (await service.SaveAsync(family)).ActiveId);
            Assert.Equal(family.Id, (await service.SaveAsync(vegan)).ActiveId);
            await service.SetActiveAsync(vegan.Id);
            var updated = CreateProfile(family.Id, "  Updated family  ", 5, "Pescatarian", ["Fish"], ["Garlic", "Mushrooms"]);
            await service.SaveAsync(updated);
            // Repeating the same update must not introduce duplicate children.
            await service.SaveAsync(updated);
        }
        await using (var provider = Open())
        {
            var book = await provider.GetRequiredService<IProfileService>().LoadAsync();
            Assert.Equal(2, book.Profiles.Count);
            Assert.Equal(vegan.Id, book.ActiveId);
            var saved = book.Profiles.Single(p => p.Id == family.Id);
            Assert.Equal("Updated family", saved.Name);
            Assert.Equal(5, saved.Servings);
            Assert.Equal("Pescatarian", saved.Diet.Name);
            Assert.Equal(new[] { "Fish" }, saved.Allergies.Select(a => a.Name));
            Assert.Equal(new[] { "Garlic", "Mushrooms" }, saved.Exclusions.Select(e => e.Name).OrderBy(n => n));
        }
    }

    [Fact]
    public async Task Default_selection_is_unique_and_editing_does_not_reset_it()
    {
        await using var provider = Open();
        var service = provider.GetRequiredService<IProfileService>();
        var first = Profile();
        var second = Profile("Second");
        Assert.True((await service.SaveAsync(first)).Active.IsDefault);
        await service.SaveAsync(second);
        await service.SetActiveAsync(second.Id);
        var edited = await service.SaveAsync(CreateProfile(second.Id, "Edited", 2, "Vegan", [], []));
        Assert.Equal(second.Id, edited.ActiveId);
        Assert.Single(edited.Profiles, p => p.IsDefault);
        await using var db = await provider.GetRequiredService<IDbContextFactory<SmartPantryDbContext>>().CreateDbContextAsync();
        var other = await db.HouseholdProfiles.SingleAsync(p => p.Id == first.Id);
        other.SetDefault(true);
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Unknown_profile_cannot_be_selected()
    {
        await using var provider = Open();
        var service = provider.GetRequiredService<IProfileService>();
        var family = Profile(); await service.SaveAsync(family);
        await Assert.ThrowsAsync<SmartPantry.Domain.Exceptions.DomainException>(() => service.SetActiveAsync(Guid.NewGuid()));
        Assert.Equal(family.Id, (await service.LoadAsync()).ActiveId);
    }

    [Fact]
    public async Task Legacy_json_is_imported_once_without_modifying_the_source()
    {
        var profile = Profile();
        var json = JsonSerializer.Serialize(new { ActiveId = profile.Id, Profiles = new[] { new { profile.Id, profile.Name, profile.Servings, Diet = profile.Diet.Name, Allergies = profile.Allergies.Select(a => a.Name), Exclusions = profile.Exclusions.Select(e => e.Name) } } });
        await File.WriteAllTextAsync(LegacyPath, json);
        await using (var provider = Open())
        {
            var book = await provider.GetRequiredService<IProfileService>().LoadAsync();
            Assert.Equal(profile.Id, book.ActiveId);
            Assert.Contains(book.Active.Exclusions, i => i.Name == "Mushrooms");
        }
        Assert.Equal(json, await File.ReadAllTextAsync(LegacyPath));
        await File.WriteAllTextAsync(LegacyPath, "invalid JSON that must never be imported again");
        await using (var provider = Open())
            Assert.Single((await provider.GetRequiredService<IProfileService>().LoadAsync()).Profiles);
    }

    [Fact]
    public async Task Imported_profiles_share_catalogue_entries_and_can_be_edited()
    {
        var first = Profile();
        var second = Profile("Other family");
        await File.WriteAllTextAsync(LegacyPath, JsonSerializer.Serialize(new
        {
            ActiveId = first.Id,
            Profiles = new[] { first, second }.Select(p => new
            {
                p.Id, p.Name, p.Servings, Diet = p.Diet.Name,
                Allergies = p.Allergies.Select(i => i.Name), Exclusions = p.Exclusions.Select(i => i.Name)
            })
        }));
        await using var provider = Open();
        var service = provider.GetRequiredService<IProfileService>();
        await service.LoadAsync();
        await service.SaveAsync(CreateProfile(first.Id, "Updated", 3, "Vegetarian", ["Eggs"], []));
        await using var db = await provider.GetRequiredService<IDbContextFactory<SmartPantryDbContext>>().CreateDbContextAsync();
        Assert.Equal(5, await db.Diets.CountAsync());
        Assert.Equal(8, await db.Ingredients.CountAsync());
        var book = await service.LoadAsync();
        Assert.Equal(2, book.Profiles.Single(p => p.Id == second.Id).Exclusions.Count);
    }

    [Fact]
    public async Task Invalid_legacy_data_rolls_back_and_allows_retry()
    {
        await File.WriteAllTextAsync(LegacyPath, "not json");
        await using var provider = Open();
        var service = provider.GetRequiredService<IProfileService>();
        await Assert.ThrowsAsync<JsonException>(() => service.LoadAsync());
        var profile = Profile();
        await File.WriteAllTextAsync(LegacyPath, JsonSerializer.Serialize(new { ActiveId = profile.Id, Profiles = new[] { new { profile.Id, profile.Name, profile.Servings, Diet = profile.Diet.Name, Allergies = profile.Allergies.Select(a => a.Name), Exclusions = profile.Exclusions.Select(e => e.Name) } } }));
        Assert.Single((await service.LoadAsync()).Profiles);
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(21)]
    public void Domain_rejects_invalid_servings(int servings) =>
        Assert.Throws<SmartPantry.Domain.Exceptions.DomainException>(() => CreateProfile(Guid.NewGuid(), "Family", servings, "Vegan", [], []));

    [Fact]
    public void Domain_rejects_invalid_names_diets_allergens_and_duplicate_ingredients()
    {
        Assert.Throws<SmartPantry.Domain.Exceptions.DomainException>(() => CreateProfile(Guid.NewGuid(), " ", 4, "Vegan", [], []));
        Assert.Throws<SmartPantry.Domain.Exceptions.DomainException>(() => CreateProfile(Guid.NewGuid(), new string('a', 51), 4, "Vegan", [], []));
        Assert.Throws<SmartPantry.Domain.Exceptions.DomainException>(() => CreateProfile(Guid.NewGuid(), "Family", 4, "invalid", [], []));
        Assert.Throws<SmartPantry.Domain.Exceptions.DomainException>(() => CreateProfile(Guid.NewGuid(), "Family", 4, "Vegan", ["None", "Eggs"], []));
        Assert.Throws<SmartPantry.Domain.Exceptions.DomainException>(() => CreateProfile(Guid.NewGuid(), "Family", 4, "Vegan", [], ["Mushrooms", " mushrooms "]));
        Assert.Throws<SmartPantry.Domain.Exceptions.DomainException>(() => CreateProfile(Guid.NewGuid(), "Family", 4, "Vegan", [], [" "]));
        var draft = new ProfileDraft(); draft.Servings.Value = "1.5";
        Assert.False(draft.Servings.Validate());
    }

    private static HouseholdProfile CreateProfile(Guid id, string name, int servings, string diet, string[] allergies, string[] exclusions)
        => new(id, name, servings, diet,
            allergies.Select(value => new SmartPantry.Domain.AggregateRoots.Ingredients.Ingredient(value)),
            exclusions.Select(value => new SmartPantry.Domain.AggregateRoots.Ingredients.Ingredient(value)));

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        Directory.Delete(directory, recursive: true);
    }
}


