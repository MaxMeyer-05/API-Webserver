using GroceryStore.Features.Suppliers;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Suppliers;

[Trait("Category", "Unit")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Suppliers")]
public class SupplierMapperTest
{
    private readonly SupplierMapper _mapper = new();

    #region ToSupplierDto Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToSupplierDto_ShouldMapAllFields_WhenLocationNavigationIsPresent()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("10115", "Berlin");
        var supplier = SupplierTestData.CreateSupplier(
            id: Guid.NewGuid(),
            companyName: "Biohof Nord",
            email: "kontakt@biohof.de",
            zipCode: location.ZipCode,
            location: location);

        // Act
        var dto = _mapper.ToSupplierDto(supplier);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(supplier.Id, dto.SupplierId);
        Assert.Equal("supplier", dto.Role);
        Assert.Equal("Biohof Nord", dto.CompanyName);
        Assert.Equal("Dorfstraße 12", dto.Address);
        Assert.Equal("Berlin, 10115", dto.Location);
        Assert.Equal(supplier.PhoneNumber, dto.PhoneNumber);
        Assert.Equal("kontakt@biohof.de", dto.Email);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToSupplierDto_ShouldHandleNullLocationNavigation()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier(location: null);

        // Act
        var dto = _mapper.ToSupplierDto(supplier);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(supplier.ZipCode, dto.Location);
    }

    #endregion

    #region ToSupplierEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToSupplierEntity_ShouldMapRegistrationDtoAndHashPasswordWithBCrypt()
    {
        // Arrange
        var registrationDto = SupplierTestData.CreateSupplierRegistrationDto(password: "MySecret123!");

        // Act
        var entity = _mapper.ToSupplierEntity(registrationDto);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(registrationDto.CompanyName, entity.CompanyName);
        Assert.Equal(registrationDto.Street, entity.Street);
        Assert.Equal(registrationDto.HouseNumber, entity.HouseNumber);
        Assert.Equal(registrationDto.ZipCode, entity.ZipCode);
        Assert.Equal(registrationDto.PhoneNumber, entity.PhoneNumber);
        Assert.Equal(registrationDto.Email, entity.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify("MySecret123!", entity.PasswordHash));
    }

    #endregion

    #region UpdateSupplierEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateSupplierEntity_ShouldUpdateProvidedFieldsAndSetUpdatedAtDateTime()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier();
        var originalUpdatedAt = supplier.UpdatedAtDateTime;

        var updateDto = new SupplierUpdateDto(
            CompanyName: "Biohof Süd",
            Street: "Südweg",
            HouseNumber: "9",
            ZipCode: "80331",
            PhoneNumber: "089123456",
            Email: "sued@biohof.de",
            Password: null,
            ConfirmPassword: null);

        // Act
        _mapper.UpdateSupplierEntity(supplier, updateDto);

        // Assert
        Assert.Equal("Biohof Süd", supplier.CompanyName);
        Assert.Equal("Südweg", supplier.Street);
        Assert.Equal("9", supplier.HouseNumber);
        Assert.Equal("80331", supplier.ZipCode);
        Assert.Equal("089123456", supplier.PhoneNumber);
        Assert.Equal("sued@biohof.de", supplier.Email);
        Assert.True(supplier.UpdatedAtDateTime >= originalUpdatedAt);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateSupplierEntity_ShouldUpdatePassword_WhenPasswordsMatch()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier();
        var updateDto = new SupplierUpdateDto(
            CompanyName: null,
            Street: null,
            HouseNumber: null,
            ZipCode: null,
            PhoneNumber: null,
            Email: null,
            Password: "NewPassword123!",
            ConfirmPassword: "NewPassword123!");

        // Act
        _mapper.UpdateSupplierEntity(supplier, updateDto);

        // Assert
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPassword123!", supplier.PasswordHash));
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateSupplierEntity_ShouldPreserveUnspecifiedFields_WhenOnlyEmailIsProvided()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier();
        var updateDto = new SupplierUpdateDto(
            CompanyName: null,
            Street: null,
            HouseNumber: null,
            ZipCode: null,
            PhoneNumber: null,
            Email: "updated@domain.de",
            Password: null,
            ConfirmPassword: null);

        // Act
        _mapper.UpdateSupplierEntity(supplier, updateDto);

        // Assert
        Assert.Equal("Biohof Nord", supplier.CompanyName);
        Assert.Equal("Dorfstraße", supplier.Street);
        Assert.Equal("12", supplier.HouseNumber);
        Assert.Equal("updated@domain.de", supplier.Email);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateSupplierEntity_ShouldThrowArgumentException_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier();
        var updateDto = new SupplierUpdateDto(
            CompanyName: null,
            Street: null,
            HouseNumber: null,
            ZipCode: null,
            PhoneNumber: null,
            Email: null,
            Password: "Password1!",
            ConfirmPassword: "MismatchPassword2!");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _mapper.UpdateSupplierEntity(supplier, updateDto));
        Assert.Equal("Password and Confirm Password do not match.", ex.Message);
    }

    #endregion

    #region AnonymizeSupplierEntity Tests

    [Fact]
    [Trait("Action", "Anonymize")]
    public void AnonymizeSupplierEntity_ShouldOverwriteSensitiveData()
    {
        // Arrange
        var supplier = SupplierTestData.CreateSupplier();
        var originalId = supplier.Id;

        // Act
        _mapper.AnonymizeSupplierEntity(supplier);

        // Assert
        Assert.Equal(originalId, supplier.Id);
        Assert.Equal("anonymized_supplier", supplier.Role);
        Assert.Equal("Anonymized Supplier", supplier.CompanyName);
        Assert.Equal("null", supplier.Street);
        Assert.Equal("null", supplier.HouseNumber);
        Assert.Null(supplier.PhoneNumber);
        Assert.StartsWith("anonymized_", supplier.Email);
        Assert.EndsWith("@system.local", supplier.Email);
        Assert.False(string.IsNullOrWhiteSpace(supplier.PasswordHash));
    }

    #endregion
}