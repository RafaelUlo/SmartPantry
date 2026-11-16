using SmartPantry.Domain;

namespace SmartPantry.Infrastructure.SqlLite;

// Infrastructure metadata only; profile selection belongs to HouseholdProfile.IsDefault.
internal sealed class LegacyImportState : BaseEntity
{
    public LegacyImportState() : base(Guid.Parse("00000000-0000-0000-0000-000000000001")) { }
    public bool LegacyImportCompleted { get; set; }
}
