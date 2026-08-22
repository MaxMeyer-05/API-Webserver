namespace GroceryStore.Models;

/// <summary>
/// Represents a data transfer object (DTO) for an allergen.
/// </summary>
/// <param name="Name">The name of the allergen.</param>
/// <param name="SupplierId">The identifier of the associated supplier.</param>
/// <param name="Ingredients">A list of ingredient references associated with the allergen.</param
public record AllergenDto(
    string Name,
    Guid SupplierId,
    IReadOnlyList<IngredientRefDto>? Ingredients = null
);

/// <summary>
/// Represents a data transfer object (DTO) for creating a new allergen.
/// </summary>
/// <param name="Name">The name of the allergen.</param>
/// <param name="SupplierId">The identifier of the associated supplier.</param>
/// <param name="IngredientIds">A list of ingredient identifiers associated with the allergen.</param>
public record AllergenCreateDto(
    string Name,
    Guid SupplierId,
    List<int>? IngredientIds
);

/// <summary>
/// Represents a data transfer object (DTO) for updating an existing allergen.
/// </summary>
/// <param name="Name">The name of the allergen.</param>
/// <param name="IngredientIds">A list of ingredient identifiers associated with the allergen.</
public record AllergenUpdateDto(
    string? Name,
    List<int>? IngredientIds
);