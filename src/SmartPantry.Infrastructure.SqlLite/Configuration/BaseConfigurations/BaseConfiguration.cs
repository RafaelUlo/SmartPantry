using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartPantry.Domain;

namespace SmartPantry.Infrastructure.SqlLite.Configuration.BaseConfigurations;

// Apply to each concrete table, not to BaseEntity as an EF inheritance hierarchy.
public class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class, IAuditableEntity
{
    public const int ActorMaxLength = 256;
    private static readonly ValueConverter<DateTime, DateTime> UtcDateTime = new(
        value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasBaseType((Type?)null);

        builder.Property("Id")
            .HasColumnOrder(0);

        builder.Property(e => e.CreatedAt)
            .HasColumnOrder(1)
            .HasConversion(UtcDateTime)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasColumnOrder(2)
            .HasMaxLength(ActorMaxLength)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnOrder(3)
            .HasConversion(UtcDateTime);

        builder.Property(e => e.UpdatedBy)
            .HasColumnOrder(4).HasMaxLength(ActorMaxLength);

        builder.Property(e => e.DeletedAt)
            .HasColumnOrder(5)
            .HasConversion(UtcDateTime);

        builder.Property(e => e.DeletedBy)
            .HasColumnOrder(6)
            .HasMaxLength(ActorMaxLength);

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}
