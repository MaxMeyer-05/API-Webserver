using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents a category used to classify recipes.
/// </summary>
[Table("categories")]
public partial class Category
{
    /// <summary>
    /// Defines the unique identifier for the category.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Defines the name of the category.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Defines the optional identifier of the supplier that owns this category.
    /// </summary>
    [Required]
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Defines the supplier associated with this category.
    /// </summary>
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// Defines the recipes assigned to this category.
    /// </summary>
    public virtual ICollection<Recipe> Recipes { get; set; } = [];
}
