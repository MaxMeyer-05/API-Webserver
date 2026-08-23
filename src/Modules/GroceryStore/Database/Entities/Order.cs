using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents an order placed by a user.
/// </summary>
[Table("orders")]
public partial class Order
{
    /// <summary>
    /// Defines the unique identifier for the order.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Defines the order number assigned to the user for this order.
    /// </summary>
    [Required]
    public int UserOrderNumber { get; set; }

    /// <summary>
    /// Defines the identifier of the user who placed the order.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Defines the date and time at which the order was placed.
    /// </summary>
    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Defines the total monetary amount of the order.
    /// </summary>
    [Required]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Defines whether the order has been canceled.
    /// </summary>
    [Required]
    public bool IsCanceled { get; set; } = false;

    /// <summary>
    /// Defines whether the order has been completed.
    /// </summary>
    [Required]
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Defines the items included in this order.
    /// </summary>
    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];

    /// <summary>
    /// Defines the user who placed this order.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
