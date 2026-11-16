namespace SmartPantry.Domain;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    string CreatedBy { get; }
    DateTime? UpdatedAt { get; }
    string? UpdatedBy { get; }
    DateTime? DeletedAt { get; }
    string? DeletedBy { get; }
}
