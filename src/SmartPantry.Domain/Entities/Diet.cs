using SmartPantry.Domain.Exceptions;

namespace SmartPantry.Domain.Entities;

public sealed class Diet : BaseEntity
{
    public string Name { get; private set; }

    public Diet(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Name cannot be empty");
        }

        Name = name;
    }
}
