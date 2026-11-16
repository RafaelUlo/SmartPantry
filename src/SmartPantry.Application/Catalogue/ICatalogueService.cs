namespace SmartPantry.Application.Catalogue;

public sealed record CatalogueOption(Guid Id, string Name);
public sealed record ProfileOptions(CatalogueOption[] Diets, CatalogueOption[] Allergens);

public interface ICatalogueService
{
    Task<ProfileOptions> GetProfileOptionsAsync(CancellationToken cancellationToken = default);
    Task<CatalogueOption[]> SearchIngredientsAsync(string search, CancellationToken cancellationToken = default);
    Task<CatalogueOption> CreateIngredientAsync(string name, CancellationToken cancellationToken = default);
}
