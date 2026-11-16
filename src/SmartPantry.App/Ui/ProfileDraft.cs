using SmartPantry.Domain.AggregateRoots.Profiles;
using SmartPantry.Domain.AggregateRoots.Ingredients;

namespace SmartPantry.Mobile.Ui;

public sealed class ProfileDraft
{
    public string[] Diets { get; private set; } = [];
    public string[] Allergens { get; private set; } = [];
    public void SetOptions(string[] diets, string[] allergens)
    {
        Diets = diets;
        Allergens = allergens;
        if (string.IsNullOrEmpty(Diet)) Diet = diets.FirstOrDefault() ?? string.Empty;
    }
    private readonly Guid id;
    public ProfileDraft(string name = "Family")
    {
        id = Guid.NewGuid(); Name.Value = name;
    }
    public ProfileDraft(HouseholdProfile profile)
    {
        id = profile.Id; Name.Value = profile.Name; Servings.Value = profile.Servings.ToString();
        Diet = profile.Diet.Name; Allergies.UnionWith(profile.Allergies.Select(a => a.Name)); Exclusions.AddRange(profile.Exclusions.Select(e => e.Name));
    }
    public FieldState<string> Name { get; } = new("", ProfileRules.NameError);
    public FieldState<string> Servings { get; } = new("4", value =>
        int.TryParse(value, out var servings) ? ProfileRules.ServingsError(servings) : "Enter a whole number from 1 to 20.");
    public string Diet { get; set; } = string.Empty;
    public HashSet<string> Allergies { get; } = new();
    public List<string> Exclusions { get; } = [];
    public HouseholdProfile Build() => new(id, Name.Value, int.Parse(Servings.Value), Diet, Allergies.Select(name => new Ingredient(name)), Exclusions.Select(name => new Ingredient(name)));
}
