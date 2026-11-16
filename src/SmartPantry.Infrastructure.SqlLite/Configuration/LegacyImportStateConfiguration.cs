using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPantry.Domain.Entities;
using SmartPantry.Domain.AggregateRoots.Profiles;
using SmartPantry.Infrastructure.SqlLite.Configuration.BaseConfigurations;

namespace SmartPantry.Infrastructure.SqlLite.Configuration;

internal sealed class LegacyImportStateConfiguration : BaseConfiguration<LegacyImportState>
{
    public override void Configure(EntityTypeBuilder<LegacyImportState> builder)
    {
        base.Configure(builder);
        builder.ToTable("LegacyImportState");
        builder.HasKey(state => state.Id);
        builder.Property(state => state.Id).ValueGeneratedNever();
    }
}
