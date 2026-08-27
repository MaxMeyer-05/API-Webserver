using System.Text.Json.Serialization;

namespace GroceryStore.Features.Customers;

/// <summary>
/// Data Transfer Object (DTO) representing a customer.
/// </summary>
/// <param name="CustomerId">The unique identifier of the customer.</param>
/// <param name="Role">The role of the customer (e.g., Admin, Customer).</param>
/// <param name="Name">The full name of the customer.</param>
/// <param name="BirthDate">The birth date of the customer.</param>
/// <param name="Email">The email address of the customer.</param>
/// <param name="PhoneNumber">The phone number of the customer (optional).</param>
/// <param name="Address">The street address of the customer.</param>
/// <param name="Location">The location (zip code and city) of the customer.</param>
/// <param name="CreatedAtDateTime">The date and time when the customer was created.</param>
/// <param name="UpdatedAtDateTime">The date and time when the customer was last updated.</param>
public record CustomerDto(
    Guid CustomerId,
    string Role,
    string Name, 
    DateOnly BirthDate, 
    string Email, 
    string? PhoneNumber, 
    string Address, 
    string Location,
    DateTime CreatedAtDateTime,
    DateTime UpdatedAtDateTime
);

/// <summary>
/// Data Transfer Object (DTO) for customer registration.
/// </summary>
/// <param name="FirstName">The first name of the customer.</param>
/// <param name="LastName">The last name of the customer.</param>
/// <param name="Email">The email address of the customer.</param>
/// <param name="BirthDate">The birth date of the customer.</param>
/// <param name="PhoneNumber">The phone number of the customer.</param>
/// <param name="Street">The street address of the customer.</param>
/// <param name="HouseNumber">The house number of the customer.</param>
/// <param name="ZipCode">The zip code of the customer's location.</param>
/// <param name="Password">The password for the customer's account.</param>
/// <param name="ConfirmPassword">The confirmation of the password for the customer's account.</param>
public record CustomerRegistrationDto(
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
/// Data Transfer Object (DTO) for updating customer information.
/// </summary>
/// <param name="FirstName">The first name of the customer.</param>
/// <param name="LastName">The last name of the customer.</param>
/// <param name="Email">The email address of the customer.</param>
/// <param name="BirthDate">The birth date of the customer.</param>
/// <param name="PhoneNumber">The phone number of the customer.</param>
/// <param name="Street">The street address of the customer.</param>
/// <param name="HouseNumber">The house number of the customer.</param>
/// <param name="ZipCode">The zip code of the customer's location.</param>
/// <param name="Password">The password for the customer's account.</param>
/// <param name="ConfirmPassword">The confirmation of the password for the customer's account.</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public record CustomerUpdateDto(
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
/// Data Transfer Object (DTO) for customer login.
/// </summary>
/// <param name="Email">The email address of the customer.</param>
/// <param name="Password">The password for the customer's account.</param>
public record CustomerLoginDto(
    string Email, 
    string Password
);

/// <summary>
/// This DTO contains the customer's ID and password for verification purposes.
/// </summary>
/// <param name="Password">The password of the customer.</param>
public record CustomerActionRequest (
    string Password
);

/// <summary>
/// Data Transfer Object (DTO) for authentication response.
/// </summary>
/// <param name="Token">The authentication token for the customer.</param>
/// <param name="CustomerId">The unique identifier of the customer.</param>
/// <param name="Role">The role of the customer.</param>
public record CustomerAuthResponseDto(
    string Token,
    Guid CustomerId,
    string Role
);