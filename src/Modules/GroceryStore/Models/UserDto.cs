namespace GroceryStore.Models;

/// <summary>
/// Data Transfer Object (DTO) representing a user.
/// </summary>
/// <param name="Role">The role of the user (e.g., Admin, Customer).</param>
/// <param name="Name">The full name of the user.</param>
/// <param name="BirthDate">The birth date of the user.</param>
/// <param name="Email">The email address of the user.</param>
/// <param name="PhoneNumber">The phone number of the user (optional).</param>
/// <param name="Address">The street address of the user.</param>
/// <param name="Location">The location (zip code and city) of the user.</param>
/// <param name="CreatedAt">The date and time when the user was created.</param>
/// <param name="UpdatedAt">The date and time when the user was last updated.</param>
public record UserDto(
    string Role,
    string Name, 
    DateOnly BirthDate, 
    string Email, 
    string? PhoneNumber, 
    string Address, 
    string Location,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>
/// Data Transfer Object (DTO) for user registration.
/// </summary>
/// <param name="FirstName">The first name of the user.</param>
/// <param name="LastName">The last name of the user.</param>
/// <param name="Email">The email address of the user.</param>
/// <param name="BirthDate">The birth date of the user.</param>
/// <param name="PhoneNumber">The phone number of the user.</param>
/// <param name="Street">The street address of the user.</param>
/// <param name="HouseNumber">The house number of the user.</param>
/// <param name="ZipCode">The zip code of the user's location.</param>
/// <param name="Password">The password for the user's account.</param>
/// <param name="ConfirmPassword">The confirmation of the password for the user's account.</param>
public record UserRegistrationDto(
    string FirstName, 
    string LastName, 
    string Email, 
    DateOnly BirthDate, 
    string? PhoneNumber, 
    string Street, 
    string HouseNumber, 
    string ZipCode, 
    string Password,
    string ConfirmPassword
);

/// <summary>
/// Data Transfer Object (DTO) for updating user information.
/// </summary>
/// <param name="FirstName">The first name of the user.</param>
/// <param name="LastName">The last name of the user.</param>
/// <param name="Email">The email address of the user.</param>
/// <param name="BirthDate">The birth date of the user.</param>
/// <param name="PhoneNumber">The phone number of the user.</param>
/// <param name="Street">The street address of the user.</param>
/// <param name="HouseNumber">The house number of the user.</param>
/// <param name="ZipCode">The zip code of the user's location.</param>
/// <param name="Password">The password for the user's account.</param>
/// <param name="ConfirmPassword">The confirmation of the password for the user's account.</param>
public record UserUpdateDto(
    string? FirstName, 
    string? LastName, 
    string? Email, 
    DateOnly? BirthDate, 
    string? PhoneNumber, 
    string? Street, 
    string? HouseNumber, 
    string? ZipCode,
    string? Password,
    string? ConfirmPassword
);

/// <summary>
/// Data Transfer Object (DTO) for user login.
/// </summary>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The password for the user's account.</param>
public record UserLoginDto(
    string Email, 
    string Password
);

/// <summary>
/// This DTO contains the user's ID and password for verification purposes.
/// </summary>
/// <param name="Password">The password of the user.</param>
public record UserActionRequest (
    string? Password
);