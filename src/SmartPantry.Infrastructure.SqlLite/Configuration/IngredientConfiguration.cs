using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPantry.Domain.AggregateRoots.Ingredients;
using SmartPantry.Infrastructure.SqlLite.Configuration.BaseConfigurations;

namespace SmartPantry.Infrastructure.SqlLite.Configuration;

internal sealed class IngredientConfiguration : BaseConfiguration<Ingredient>
{
    public override void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        base.Configure(builder);
        builder.ToTable("Ingredients", table => table.HasCheckConstraint("CK_Ingredients_Name", "length(trim(Name)) > 0"));
        builder.HasKey(ingredient => ingredient.Id);
        builder.Property(ingredient => ingredient.Name).IsRequired();
        builder.Property(ingredient => ingredient.Calories).IsRequired();

        // Fixed IDs and audit dates keep HasData stable across model builds.
        builder.HasData(
            new { Id = Guid.Parse("75ea73c3-add9-497c-894c-96b4df59bb59"), Name = "Milk / Dairy", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty, Calories = 0 },
            new { Id = Guid.Parse("b0dd237b-2b46-4e00-b767-0a84ae758496"), Name = "Eggs", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty, Calories = 0 },
            new { Id = Guid.Parse("44608ed1-cb3e-4bfe-b2bb-81f992b6d71e"), Name = "Peanuts", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty, Calories = 0 },
            new { Id = Guid.Parse("d9f41a73-36a7-4178-a329-cf6eb4f0309b"), Name = "Tree nuts", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty, Calories = 0 },
            new { Id = Guid.Parse("686266ca-56ab-4545-b6f5-0df8be99a85f"), Name = "Fish", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty, Calories = 0 },
            new { Id = Guid.Parse("89e5fffd-9810-4631-8c77-23c393bacc6c"), Name = "Shellfish", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty, Calories = 0 });
    }
}
