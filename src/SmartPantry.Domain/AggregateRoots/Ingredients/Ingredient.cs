namespace SmartPantry.Domain.AggregateRoots.Ingredients;

public sealed class Ingredient : BaseEntity, IAggreagateRoot
{
    public Ingredient(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("An ingredient name is required.", nameof(name));
        Name = name.Trim();
    }

    public string Name { get; private set; }

    public int Calories { get; private set; }
}
