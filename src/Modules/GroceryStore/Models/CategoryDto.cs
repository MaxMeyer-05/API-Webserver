namespace GroceryStore.Models;

/// <summary>
/// Represents a data transfer object (DTO) for a category.
/// </summary>
/// <param name="Name">The name of the category.</param>
/// <param name="SupplierId">The identifier of the associated supplier.</param>
/// <param name="Recipes">A list of recipe references associated with the category.</param>
public record CategoryDto(
    string Name,
    Guid SupplierId,
    IReadOnlyList<RecipeRefDto>? Recipes = null
);

/// <summary>
/// Represents a data transfer object (DTO) for creating a new category.
/// </summary>
/// <param name="Name">The name of the category.</param>
/// <param name="SupplierId">The identifier of the associated supplier.</param>
/// <param name="RecipeIds">A list of recipe identifiers associated with the category.</param>
public record CategoryCreateDto(
    string Name,
    Guid SupplierId,
    List<int>? RecipeIds
);

/// <summary>
/// Represents a data transfer object (DTO) for updating an existing category.
/// </summary>
/// <param name="Name">The name of the category.</param>
/// <param name="RecipeIds">A list of recipe identifiers associated with the category.</param>
public record CategoryUpdateDto(
    string? Name,
    List<int>? RecipeIds
);