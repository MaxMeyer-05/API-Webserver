using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents a customer in the grocery store system,
/// containing personal and contact information, 
/// as well as authentication details.
/// </summary>
[Table("customers")]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(PhoneNumber), IsUnique = true)]
public partial class Customer
{
    /// <summary>
    /// Defines the unique identifier for the customer.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Defines the role of the customer,
    /// which can be used for authorization purposes.
    /// </summary>
    [Required]
    public string Role { get; set; } = "customer";

    /// <summary>
    /// Defines the last name of the customer.
    /// </summary>
    [Required]
    public string LastName { get; set; } = null!;
    
    /// <summary>
    /// Defines the first name of the customer.
    /// </summary>
    [Required]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Defines the hashed password of the customer for authentication purposes.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Defines the email address of the customer,
    /// which can be used for communication and login purposes.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Defines the phone number of the customer,
    /// which can be used for communication purposes.
    /// </summary>
    /// <remarks>
    /// This field is optional and can be null if the customer does not provide a phone number.
    /// </remarks>
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Defines the birth date of the customer,
    /// which can be used for age verification and personalization purposes.
    /// </summary>
    [Required]
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Defines the street address of the customer,
    /// which can be used for shipping and billing purposes.
    /// </summary>
    [Required]
    public string Street { get; set; } = null!;

    /// <summary>
    /// Defines the house number of the customer,
    /// which can be used for shipping and billing purposes.
    /// </summary>
    [Required]
    public string HouseNumber { get; set; } = null!;

    /// <summary>
    /// Defines the zip code of the customer,
    /// which can be used for shipping and billing purposes.
    /// </summary>
    [Required]
    public string ZipCode { get; set; } = null!;

    /// <summary>
    /// Defines the collection of orders associated with the customer,
    /// which can be used to track the customer's purchase history and order status.
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = [];

    /// <summary>
    /// Defines the navigation property for the zip code, 
    /// which can be used to access additional information about the customer's location.
    /// </summary>
    [ForeignKey("ZipCode")]
    public virtual Location ZipCodeNavigation { get; set; } = null!;

    /// <summary>
    /// Defines the date and time when the customer was created,
    /// which can be used for auditing and tracking purposes.
    /// </summary>
    [Required]
    public DateTime CreatedAtDateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Defines the date and time when the customer was last updated,
    /// which can be used for auditing and tracking purposes.
    /// </summary>
    [Required]
    public DateTime UpdatedAtDateTime { get; set; } = DateTime.UtcNow;
}
