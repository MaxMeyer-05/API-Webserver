using GroceryStore.Database.Entities;

using GroceryStore.Features.Suppliers;

namespace GroceryStore.Tests.Features.Suppliers;

public static class SupplierTestData
{
    #region Entity Fixtures

    public static Supplier CreateSupplier(
        Guid? id = null,
        string companyName = "Biohof Nord",
        string email = "kontakt@biohof-nord.de",
        string zipCode = "10115",
        string passwordHash = "$2a$11$e8.Z9bW9jB4X8o2tYJ7H7u6m6E9Z8bW9jB4X8o2tYJ7H7u6m6E9Z8",
        Location? location = null,
        string? phoneNumber = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Role = "supplier",
        CompanyName = companyName,
        Email = email,
        PasswordHash = passwordHash,
        PhoneNumber = phoneNumber,
        Street = "Dorfstraße",
        HouseNumber = "12",
        ZipCode = zipCode,
        ZipCodeNavigation = location,
        CreatedAtDateTime = DateTime.UtcNow,
        UpdatedAtDateTime = DateTime.UtcNow
    };

    #endregion

    #region DTO Fixtures

    public static SupplierDto CreateSupplierDto(
        Guid? supplierId = null,
        string companyName = "Biohof Nord",
        string email = "kontakt@biohof-nord.de",
        string role = "supplier") => new(
        SupplierId: supplierId ?? Guid.NewGuid(),
        Role: role,
        CompanyName: companyName,
        Address: "Dorfstraße 12",
        Location: "Berlin, 10115",
        PhoneNumber: "015112345678",
        Email: email,
        CreatedAtDateTime: DateTime.UtcNow,
        UpdatedAtDateTime: DateTime.UtcNow);

    public static SupplierRegistrationDto CreateSupplierRegistrationDto(
        string companyName = "Frische Paradies",
        string email = "info@frischeparadies.de",
        string password = "SecurePassword123!",
        string confirmPassword = "SecurePassword123!") => new(
        CompanyName: companyName,
        Street: "Marktweg",
        HouseNumber: "5",
        ZipCode: "10115",
        PhoneNumber: "017098765432",
        Email: email,
        Password: password,
        ConfirmPassword: confirmPassword);

    public static SupplierUpdateDto CreateSupplierUpdateDto(
        string? companyName = "Frische Paradies GmbH",
        string? street = "Neuer Marktweg",
        string? houseNumber = "5a",
        string? zipCode = "10115",
        string? phoneNumber = "017011223344",
        string? email = "kontakt@frischeparadies.de",
        string? password = null,
        string? confirmPassword = null) => new(
        CompanyName: companyName,
        Street: street,
        HouseNumber: houseNumber,
        ZipCode: zipCode,
        PhoneNumber: phoneNumber,
        Email: email,
        Password: password,
        ConfirmPassword: confirmPassword);

    public static SupplierLoginDto CreateSupplierLoginDto(
        string email = "kontakt@biohof-nord.de",
        string password = "SecretPassword123!") => new(
        Email: email,
        Password: password);

    #endregion
}