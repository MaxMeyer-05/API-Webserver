namespace GroceryStore.Features.Suppliers;

/// <summary>
/// Represents a data transfer object (DTO) for a supplier
/// </summary>
/// <param name="Role">The role of the supplier.</param>
/// <param name="CompanyName">The company name of the supplier.</param>
/// <param name="Address">The address of the supplier.</param>
/// <param name="Location">The location of the supplier.</param>
/// <param name="PhoneNumber">The phone number of the supplier.</param>
/// <param name="Email">The email address of the supplier.</param>
/// <param name="CreatedAtDateTime">The date and time when the supplier was created.</param>
/// <param name="UpdatedAtDateTime">The date and time when the supplier was last updated.</param>
public record SupplierDto(
    string Role,
    string CompanyName, 
    string Address, 
    string Location, 
    string? PhoneNumber, 
    string Email,
    DateTime CreatedAtDateTime,
    DateTime UpdatedAtDateTime
);

/// <summary>
/// Represents a data transfer object (DTO) for creating a new supplier
/// </summary>
/// <param name="CompanyName">The company name of the supplier.</param>
/// <param name="Street">The street address of the supplier.</param>
/// <param name="HouseNumber">The house number of the supplier.</param>
/// <param name="ZipCode">The zip code of the supplier.</param>
/// <param name="PhoneNumber">The phone number of the supplier.</param>
/// <param name="Email">The email address of the supplier.</param>
/// <param name="Password">The password of the supplier.</param>
/// <param name="ConfirmPassword">The confirmation of the password of the supplier.</param>
public record SupplierRegistrationDto(
    string CompanyName, 
    string Street, 
    string HouseNumber, 
    string ZipCode,
    string? PhoneNumber, 
    string Email,
    string Password,
    string ConfirmPassword
);

/// <summary>
/// Represents a data transfer object (DTO) for updating an existing supplier
/// </summary>
/// <param name="CompanyName">The company name of the supplier.</param>
/// <param name="Street">The street address of the supplier.</param>
/// <param name="HouseNumber">The house number of the supplier.</param>
/// <param name="ZipCode">The zip code of the supplier.</param>
/// <param name="PhoneNumber">The phone number of the supplier.</param>
/// <param name="Email">The email address of the supplier.</param>
public record SupplierUpdateDto(
    string? CompanyName, 
    string? Street, 
    string? HouseNumber, 
    string? ZipCode, 
    string? PhoneNumber, 
    string? Email,
    string? Password,
    string? ConfirmPassword
);

/// <summary>
/// Represents a data transfer object (DTO) for supplier login
/// </summary>
/// <param name="Email">The email address of the supplier.</param>
/// <param name="Password">The password of the supplier.</param>
public record SupplierLoginDto(
    string Email, 
    string Password
);

/// <summary>
/// This DTO contains the supplier's password for verification purposes.
/// </summary>
/// <param name="Password">The password of the supplier.</param>
public record SupplierActionRequest(
    string Password
);