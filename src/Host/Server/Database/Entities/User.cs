using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Database.Entities;

/// <summary>
/// Represents a user entity in the database.
/// </summary>
[Table("users")]
public class User
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
    /// Defines the last name of the user, 
    /// which can be used for identification and personalization purposes.
    /// </summary>
    [Required]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Defines the first name of the user, 
    /// which can be used for identification and personalization purposes.
    /// </summary>
    [Required]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Defines the email address of the user, 
    /// which can be used for communication and login purposes.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Defines the hashed password of the user for authentication purposes.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Defines the date and time when the user was created in the system.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Defines the date and time when the user was last updated in the system.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }
}