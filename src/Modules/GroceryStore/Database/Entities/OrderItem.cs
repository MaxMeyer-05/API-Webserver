using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents an ingredient and its quantity within an order.
/// </summary>
[Table("order_items")]
public partial class OrderItem
{
    /// <summary>
    /// Defines the unique identifier for the order item.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Defines the identifier of the associated order.
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    /// <summary>
    /// Defines the identifier of the ordered ingredient.
    /// </summary>
    [Required]
    public int IngredientId { get; set; }

    /// <summary>
    /// Defines the quantity of the ingredient in the order.
    /// </summary>
    [Required]
    public int Quantity { get; set; }

    /// <summary>
    /// Defines the ingredient included in the order.
    /// </summary>
    [ForeignKey(nameof(IngredientId))]
    public virtual Ingredient Ingredient { get; set; } = null!;

    /// <summary>
    /// Defines the order that contains this item.
    /// </summary>
    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;
}
