using SmartPantry.Domain.AggregateRoots.Ingredients;
using SmartPantry.Domain.Entities;
using SmartPantry.Domain.Exceptions;

namespace SmartPantry.Domain.AggregateRoots.Profiles;

public sealed class HouseholdProfile : BaseEntity, IAggreagateRoot
{
    // EF materializes scalar values before populating navigation properties.
    private HouseholdProfile()
    {
        Name = null!;
        Diet = null!;
    }

    public string Name { get; private set; }

    public bool IsDefault { get; private set; }

    public int Servings { get; private set; }

    public Guid DietId { get; private set; }

    public Diet Diet { get; private set; }

    public ICollection<Ingredient> Allergies { get; } = new HashSet<Ingredient>();

    public ICollection<Ingredient> Exclusions { get; } = new HashSet<Ingredient>();

    public HouseholdProfile(Guid id, string name, int servings, Diet diet, IEnumerable<Ingredient> allergies, IEnumerable<Ingredient> exclusions) : base(id)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("A profile identifier is required.");
        }

        if (ProfileRules.NameError(name) is { } nameError)
        {
            throw new DomainException(nameError);
        }

        if (ProfileRules.ServingsError(servings) is { } servingsError)
        {
            throw new DomainException(servingsError);
        }

        Name = name.Trim();
        Servings = servings;
        ArgumentNullException.ThrowIfNull(diet);
        Diet = diet;
        DietId = diet.Id;
        Allergies = [.. allergies];
        Exclusions = [.. exclusions];
    }

    public HouseholdProfile(string name, int servings, Diet diet, IEnumerable<Ingredient> allergies, IEnumerable<Ingredient> exclusions) : base()
    {
        if (ProfileRules.NameError(name) is { } nameError)
        {
            throw new DomainException(nameError);
        }

        if (ProfileRules.ServingsError(servings) is { } servingsError)
        {
            throw new DomainException(servingsError);
        }

        Name = name.Trim();
        Servings = servings;
        ArgumentNullException.ThrowIfNull(diet);
        Diet = diet;
        DietId = diet.Id;
        Allergies = [.. allergies];
        Exclusions = [.. exclusions];
    }

    public HouseholdProfile(Guid id, string name, int servings, string diet, IEnumerable<Ingredient> allergies, IEnumerable<Ingredient> exclusions)
        : this(id, name, servings, new Diet(diet), allergies, exclusions) { }



    public void Update(string name, int servings, Diet diet, IEnumerable<Ingredient> allergies, IEnumerable<Ingredient> exclusions)
    {
        ArgumentNullException.ThrowIfNull(diet);

        var selectedAllergies = allergies.ToArray();

        var selectedExclusions = exclusions.ToArray();

        if (ProfileRules.NameError(name) is { } nameError)
        {
            throw new DomainException(nameError);
        }

        if (ProfileRules.ServingsError(servings) is { } servingsError)
        {
            throw new DomainException(servingsError);
        }

        Name = name.Trim();
        Servings = servings;
        Diet = diet;
        DietId = diet.Id;
        Allergies.Clear();

        foreach (var ingredient in selectedAllergies)
        {
            Allergies.Add(ingredient);
        }

        Exclusions.Clear();

        foreach (var ingredient in selectedExclusions)
        {
            Exclusions.Add(ingredient);
        }
    }


    public void SetDefault(bool isDefault) => IsDefault = isDefault;
}
