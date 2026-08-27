using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents a supplier entity in the grocery store database, 
/// which contains information about suppliers providing ingredients to the store.
/// </summary>
[Table("suppliers")]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(PhoneNumber), IsUnique = true)]
public partial class Supplier
{
    /// <summary>
    /// Defines the unique identifier for the supplier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Defines the role of the supplier, 
    /// which is required and defaults to "Supplier".
    /// </summary>
    [Required]
    public string Role { get; set; } = "supplier";

    /// <summary>
    /// Defines the company name of the supplier.
    /// </summary>
    [Required]
    public string CompanyName { get; set; } = null!;

    /// <summary>
    /// Defines the hashed password of the user for authentication purposes.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Defines the phone number of the supplier, 
    /// which can be used for communication purposes.
    /// </summary>
    /// <remarks>
    /// This field is optional and can be null if the supplier does not provide a phone number.
    /// </remarks>
    [Phone(ErrorMessage = "The phone number is not valid.")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Defines the email address of the supplier, 
    /// which can be used for communication and login purposes.
    /// </summary>
    [Required(ErrorMessage = "The email address is required.")]
    [EmailAddress(ErrorMessage = "The email address is not valid.")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Defines the street address of the supplier, 
    /// which can be used for shipping and billing purposes.
    /// </summary>
    [Required]
    public string Street { get; set; } = null!;

    /// <summary>
    /// Defines the house number of the supplier, 
    /// which can be used for shipping and billing purposes.
    /// </summary>
    [Required]
    public string HouseNumber { get; set; } = null!;

    /// <summary>
    /// Defines the zip code of the supplier, 
    /// which can be used for shipping and billing purposes.
    /// </summary>
    [Required]
    public string ZipCode { get; set; } = null!;

    /// <summary>
    /// Defines the collection of ingredients associated with the supplier, 
    /// which can be used to track the supplier's inventory and product offerings.
    /// </summary>
    public virtual ICollection<Ingredient> Ingredients { get; set; } = [];

    /// <summary>
    /// Defines the collection of recipes associated with the supplier,
    /// which can be used to track the supplier's recipe offerings.
    /// </summary>
    public virtual ICollection<Recipe> Recipes { get; set; } = [];

    /// <summary>
    /// Defines the navigation property for the zip code, 
    /// which can be used to access additional information about the supplier's location.
    /// </summary>
    [ForeignKey("ZipCode")]
    public virtual Location? ZipCodeNavigation { get; set; }

    /// <summary>
    /// Defines the date and time when the supplier was created, 
    /// which can be used for auditing and tracking purposes.
    /// </summary>
    [Required]
    public DateTime CreatedAtDateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Defines the date and time when the supplier was last updated, 
    /// which can be used for auditing and tracking purposes.
    /// </summary>
    [Required]
    public DateTime UpdatedAtDateTime { get; set; } = DateTime.UtcNow;
}
