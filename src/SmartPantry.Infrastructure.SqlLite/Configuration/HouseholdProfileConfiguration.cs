using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPantry.Domain.AggregateRoots.Ingredients;
using SmartPantry.Domain.AggregateRoots.Profiles;
using SmartPantry.Infrastructure.SqlLite.Configuration.BaseConfigurations;

namespace SmartPantry.Infrastructure.SqlLite.Configuration;

internal sealed class HouseholdProfileConfiguration : BaseConfiguration<HouseholdProfile>
{
    public override void Configure(EntityTypeBuilder<HouseholdProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("HouseholdProfiles", table =>
        {
            table.HasCheckConstraint("CK_HouseholdProfiles_Name", "length(trim(Name)) BETWEEN 1 AND 50");
            table.HasCheckConstraint("CK_HouseholdProfiles_Servings", "Servings BETWEEN 1 AND 20");
        });
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.IsDefault).IsRequired();
        builder.HasIndex(profile => profile.IsDefault).IsUnique()
            .HasFilter("IsDefault = 1 AND DeletedAt IS NULL");
        builder.Property(profile => profile.Name).IsRequired().HasMaxLength(ProfileRules.MaxNameLength);
        builder.Property(profile => profile.Servings).IsRequired();

        builder.HasOne(profile => profile.Diet)
            .WithMany()
            .HasForeignKey(profile => profile.DietId)
            .OnDelete(DeleteBehavior.Restrict);

        // These are independent associations to the same ingredient catalogue.
        builder.HasMany(profile => profile.Allergies)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("HouseholdProfileAllergies",
                right => right.HasOne<Ingredient>().WithMany().HasForeignKey("IngredientId").OnDelete(DeleteBehavior.Restrict),
                left => left.HasOne<HouseholdProfile>().WithMany().HasForeignKey("HouseholdProfileId").OnDelete(DeleteBehavior.Cascade),
                join => { join.HasKey("HouseholdProfileId", "IngredientId"); });

        builder.HasMany(profile => profile.Exclusions)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("HouseholdProfileExclusions",
                right => right.HasOne<Ingredient>().WithMany().HasForeignKey("IngredientId").OnDelete(DeleteBehavior.Restrict),
                left => left.HasOne<HouseholdProfile>().WithMany().HasForeignKey("HouseholdProfileId").OnDelete(DeleteBehavior.Cascade),
                join => { join.HasKey("HouseholdProfileId", "IngredientId"); });
    }
}

