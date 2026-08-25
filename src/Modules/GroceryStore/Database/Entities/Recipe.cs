using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents a recipe that can be prepared using various ingredients.
/// </summary>
[Table("recipes")]
public partial class Recipe
{
    /// <summary>
    /// Defines the unique identifier for the recipe.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Defines how many recipes a supplier has.
    /// </summary>
    [Required]
    public int SupplierRecipeCount { get; set; }

    /// <summary>
    /// Defines the name of the recipe.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Defines the optional preparation instructions.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Defines the optional preparation time in minutes.
    /// </summary>
    public int? PreparationTime { get; set; }

    /// <summary>
    /// Defines the optional identifier of the recipe supplier.
    /// </summary>
    [Required]
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Defines the supplier that provides this recipe.
    /// </summary>
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// Defines the ingredients and quantities required by this recipe.
    /// </summary>
    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = [];

    /// <summary>
    /// Defines the categories assigned to this recipe.
    /// </summary>
    public virtual ICollection<Category> Categories { get; set; } = [];
}
