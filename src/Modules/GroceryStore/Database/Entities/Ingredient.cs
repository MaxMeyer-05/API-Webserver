using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents an ingredient that can be stocked, ordered, and used in recipes.
/// </summary>
[Table("ingredients")]
public partial class Ingredient
{
    /// <summary>
    /// Defines the unique identifier for the ingredient.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Defines the name of the ingredient.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Defines the unit used to measure the ingredient.
    /// </summary>
    [Required]
    public string Unit { get; set; } = null!;

    /// <summary>
    /// Defines the net price of one unit of the ingredient.
    /// </summary>
    [Required]
    public decimal NetPrice { get; set; }

    /// <summary>
    /// Defines the current quantity in stock.
    /// </summary>
    [Required]
    public int Stock { get; set; }

    /// <summary>
    /// Defines the identifier of the supplying company.
    /// </summary>
    [Required]
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Defines the calorie content of the ingredient.
    /// </summary>
    public decimal? Calories { get; set; }

    /// <summary>
    /// Defines the carbohydrate content of the ingredient.
    /// </summary>
    public decimal? Carbohydrates { get; set; }

    /// <summary>
    /// Defines the protein content of the ingredient.
    /// </summary>
    public decimal? Protein { get; set; }

    /// <summary>
    /// Defines the order items that reference this ingredient.
    /// </summary>
    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];

    /// <summary>
    /// Defines the recipe ingredients that reference this ingredient.
    /// </summary>
    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = [];

    /// <summary>
    /// Defines the supplier that provides this ingredient.
    /// </summary>
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// Defines the allergens contained in this ingredient.
    /// </summary>
    public virtual ICollection<Allergen> Allergens { get; set; } = [];
}
