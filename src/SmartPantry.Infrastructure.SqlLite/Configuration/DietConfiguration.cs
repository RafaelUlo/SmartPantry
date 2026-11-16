using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPantry.Domain.Entities;
using SmartPantry.Infrastructure.SqlLite.Configuration.BaseConfigurations;

namespace SmartPantry.Infrastructure.SqlLite.Configuration;

internal sealed class DietConfiguration : BaseConfiguration<Diet>
{
    public override void Configure(EntityTypeBuilder<Diet> builder)
    {
        base.Configure(builder);
        builder.ToTable("Diets", table => table.HasCheckConstraint("CK_Diets_Name", "length(trim(Name)) > 0"));

        builder.Property(diet => diet.Name)
            .IsRequired();

        // Fixed IDs and audit dates keep HasData stable across model builds.
        builder.HasData(
            new { Id = Guid.Parse("ba940e9a-f5dc-436b-9aec-427d87279eab"), Name = "No specific restriction", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty },
            new { Id = Guid.Parse("cc91e38d-96b3-4b78-9324-835093f80ca8"), Name = "Vegetarian", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty },
            new { Id = Guid.Parse("267fa4c3-ce74-4ee9-ad64-c26fbca85b0c"), Name = "Vegan", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty },
            new { Id = Guid.Parse("1d08a48a-ce28-4822-b99e-8f2267cc8ac6"), Name = "Pescatarian", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty },
            new { Id = Guid.Parse("26e71575-0e24-4bfc-8383-a4e1d3de3240"), Name = "Other", CreatedAt = new DateTime(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc), CreatedBy = string.Empty });
    }
}
