using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents a user in the grocery store system, 
/// containing personal and contact information, 
/// as well as authentication details.
/// </summary>
[Table("users")]
public partial class User
{
    /// <summary>
    /// Defines the unique identifier for the user.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Defines the role of the user, 
    /// which can be used for authorization purposes.
    /// </summary>
    [Required]
    public string Role { get; set; } = "user";

    /// <summary>
    /// Defines the last name of the user.
    /// </summary>
    [Required]
    public string LastName { get; set; } = null!;
    
    /// <summary>
    /// Defines the first name of the user.
    /// </summary>
    [Required]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Defines the hashed password of the user for authentication purposes.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Defines the email address of the user, 
    /// which can be used for communication and login purposes.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Defines the phone number of the user, 
    /// which can be used for communication purposes.
    /// </summary>
    /// <remarks>
    /// This field is optional and can be null if the user does not provide a phone number.
    /// </remarks>
    [Phone]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Defines the birth date of the user, 
    /// which can be used for age verification and personalization purposes.
    /// </summary>
    [Required]
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Defines the street address of the user, 
    /// which can be used for shipping and billing purposes.
    /// </summary>
    [Required]
    public string Street { get; set; } = null!;

    /// <summary>
    /// Defines the house number of the user, 
    /// which can be used for shipping and billing purposes.
    /// </summary>
    [Required]
    public string HouseNumber { get; set; } = null!;

    /// <summary>
    /// Defines the zip code of the user, 
    /// which can be used for shipping and billing purposes.
    /// </summary>
    [Required]
    public string ZipCode { get; set; } = null!;

    /// <summary>
    /// Defines the collection of orders associated with the user, 
    /// which can be used to track the user's purchase history and order status.
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = [];

    /// <summary>
    /// Defines the navigation property for the zip code, 
    /// which can be used to access additional information about the user's location.
    /// </summary>
    [ForeignKey("ZipCode")]
    public virtual Location ZipCodeNavigation { get; set; } = null!;

    /// <summary>
    /// Defines the date and time when the user was created, 
    /// which can be used for auditing and tracking purposes.
    /// </summary>
    [Required]
    public DateTime CreatedAtDateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Defines the date and time when the user was last updated, 
    /// which can be used for auditing and tracking purposes.
    /// </summary>
    [Required]
    public DateTime UpdatedAtDateTime { get; set; } = DateTime.UtcNow;
}
