namespace SmartPantry.Domain;

public abstract class BaseEntity : IAuditableEntity
{
    public Guid Id { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;

    public DateTime? UpdatedAt { get; private set; }

    public string? UpdatedBy { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public string? DeletedBy { get; private set; }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    // Persistence restores metadata; application code cannot fabricate audit timestamps.
    internal void RestoreAudit(IAuditableEntity source)
    {
        CreatedAt = source.CreatedAt;
        CreatedBy = source.CreatedBy;
        UpdatedAt = source.UpdatedAt;
        UpdatedBy = source.UpdatedBy;
        DeletedAt = source.DeletedAt;
        DeletedBy = source.DeletedBy;
    }
}
