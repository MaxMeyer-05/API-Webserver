using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents an allergen that can be associated with ingredients.
/// </summary>
[Table("allergens")]
public partial class Allergen
{
    /// <summary>
    /// Defines the unique identifier for the allergen.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Defines the name of the allergen.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Defines the identifier of the associated supplier.
    /// </summary>
    [Required]
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Defines the supplier associated with this allergen.
    /// </summary>
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// Defines the ingredients that contain this allergen.
    /// </summary>
    public virtual ICollection<Ingredient> Ingredients { get; set; } = [];
}
