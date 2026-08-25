using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents a location identified by its postal code.
/// </summary>
[Table("locations")]
[Index(nameof(ZipCode), IsUnique = true)]
public partial class Location
{
    /// <summary>
    /// Defines the postal code that uniquely identifies the location.
    /// </summary>
    [Key]
    public string ZipCode { get; set; } = null!;

    /// <summary>
    /// Defines the city for the postal code.
    /// </summary>
    [Required]
    public string City { get; set; } = null!;

    /// <summary>
    /// Defines the suppliers located in this postal code area.
    /// </summary>
    public virtual ICollection<Supplier> Suppliers { get; set; } = [];

    /// <summary>
    /// Defines the users located in this postal code area.
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = [];
}
