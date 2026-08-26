using GroceryStore.Database.Entities;

using GroceryStore.Features.Customers;

namespace GroceryStore.Tests.Features.Customers;

public static class CustomerTestData
{
    #region Entity Fixtures

    public static Customer CreateCustomer(
        Guid? id = null,
        string firstName = "Anna",
        string lastName = "Meier",
        string email = "anna.meier@example.com",
        string zipCode = "10115",
        string passwordHash = "$2a$11$e8.Z9bW9jB4X8o2tYJ7H7u6m6E9Z8bW9jB4X8o2tYJ7H7u6m6E9Z8",
        Location? location = null,
        string? phoneNumber = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Role = "customer",
        FirstName = firstName,
        LastName = lastName,
        Email = email,
        PhoneNumber = phoneNumber,
        BirthDate = new DateOnly(1995, 5, 20),
        Street = "Hauptstraße",
        HouseNumber = "4a",
        ZipCode = zipCode,
        ZipCodeNavigation = location!,
        PasswordHash = passwordHash,
        CreatedAtDateTime = DateTime.UtcNow,
        UpdatedAtDateTime = DateTime.UtcNow
    };

    #endregion

    #region DTO Fixtures

    public static CustomerDto CreateCustomerDto(
        Guid? customerId = null,
        string firstName = "Anna",
        string lastName = "Meier",
        string email = "anna.meier@example.com",
        string role = "customer") => new(
        CustomerId: customerId ?? Guid.NewGuid(),
        Role: role,
        Name: $"{firstName} {lastName}",
        BirthDate: new DateOnly(1995, 5, 20),
        Email: email,
        PhoneNumber: "015198765432",
        Address: "Hauptstraße 4a",
        Location: "Berlin, 10115",
        CreatedAtDateTime: DateTime.UtcNow,
        UpdatedAtDateTime: DateTime.UtcNow);

    public static CustomerRegistrationDto CreateCustomerRegistrationDto(
        string firstName = "Max",
        string lastName = "Mustermann",
        string email = "max.mustermann@example.com",
        string password = "SecurePassword123!",
        string confirmPassword = "SecurePassword123!") => new(
        FirstName: firstName,
        LastName: lastName,
        Email: email,
        BirthDate: new DateOnly(1990, 1, 15),
        PhoneNumber: "017012345678",
        Street: "Musterstraße",
        HouseNumber: "10",
        ZipCode: "10115",
        Password: password,
        ConfirmPassword: confirmPassword);

    public static CustomerUpdateDto CreateCustomerUpdateDto(
        string? firstName = "Maximilian",
        string? lastName = "Mustermann",
        string? email = "m.mustermann@example.com",
        string? phoneNumber = "017087654321",
        string? password = null,
        string? confirmPassword = null) => new(
        FirstName: firstName,
        LastName: lastName,
        Email: email,
        BirthDate: new DateOnly(1990, 1, 15),
        PhoneNumber: phoneNumber,
        Street: "Musterweg",
        HouseNumber: "12",
        ZipCode: "10115",
        Password: password,
        ConfirmPassword: confirmPassword);

    public static CustomerLoginDto CreateCustomerLoginDto(
        string email = "anna.meier@example.com",
        string password = "SecretPassword123!") => new(
        Email: email,
        Password: password);

    #endregion
}