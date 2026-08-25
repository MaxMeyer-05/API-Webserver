using GroceryStore.Features.Users;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Users;

[Trait("Category", "Unit")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Users")]
public class UserMapperTest
{
    private readonly UserMapper _mapper = new();

    #region ToUserDto Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToUserDto_ShouldMapAllFields_WhenLocationNavigationIsPresent()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation("10115", "Berlin");
        var user = UserTestData.CreateUser(
            id: Guid.NewGuid(),
            firstName: "Anna",
            lastName: "Meier",
            email: "anna.meier@example.com",
            zipCode: location.ZipCode,
            location: location,
            phoneNumber: "015198765432");

        // Act
        var dto = _mapper.ToUserDto(user);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(user.Id, dto.UserId);
        Assert.Equal("user", dto.Role);
        Assert.Equal("Anna Meier", dto.Name);
        Assert.Equal(new DateOnly(1995, 5, 20), dto.BirthDate);
        Assert.Equal("anna.meier@example.com", dto.Email);
        Assert.Equal("015198765432", dto.PhoneNumber);
        Assert.Equal("Hauptstraße 4a", dto.Address);
        Assert.Equal("Berlin, 10115", dto.Location);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToUserDto_ShouldHandleNullLocationNavigation()
    {
        // Arrange
        var user = UserTestData.CreateUser(location: null);

        // Act
        var dto = _mapper.ToUserDto(user);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(", ", dto.Location);
    }

    #endregion

    #region ToUserEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void ToUserEntity_ShouldMapRegistrationDtoAndHashPasswordWithBCrypt()
    {
        // Arrange
        var registrationDto = UserTestData.CreateUserRegistrationDto(password: "UserPassword123!");

        // Act
        var entity = _mapper.ToUserEntity(registrationDto);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(registrationDto.FirstName, entity.FirstName);
        Assert.Equal(registrationDto.LastName, entity.LastName);
        Assert.Equal(registrationDto.Email, entity.Email);
        Assert.Equal(registrationDto.BirthDate, entity.BirthDate);
        Assert.Equal(registrationDto.PhoneNumber, entity.PhoneNumber);
        Assert.Equal(registrationDto.Street, entity.Street);
        Assert.Equal(registrationDto.HouseNumber, entity.HouseNumber);
        Assert.Equal(registrationDto.ZipCode, entity.ZipCode);
        Assert.True(BCrypt.Net.BCrypt.Verify("UserPassword123!", entity.PasswordHash));
    }

    #endregion

    #region UpdateUserEntity Tests

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateUserEntity_ShouldUpdateProvidedFieldsAndSetUpdatedAtDateTime()
    {
        // Arrange
        var user = UserTestData.CreateUser();
        var originalUpdatedAt = user.UpdatedAtDateTime;

        var updateDto = new UserUpdateDto(
            FirstName: "Erika",
            LastName: "Musterfrau",
            Email: "erika.m@example.com",
            BirthDate: new DateOnly(1992, 3, 10),
            PhoneNumber: "0170555666",
            Street: "Neuestraße",
            HouseNumber: "7b",
            ZipCode: "20095",
            Password: null,
            ConfirmPassword: null);

        // Act
        _mapper.UpdateUserEntity(user, updateDto);

        // Assert
        Assert.Equal("Erika", user.FirstName);
        Assert.Equal("Musterfrau", user.LastName);
        Assert.Equal("erika.m@example.com", user.Email);
        Assert.Equal(new DateOnly(1992, 3, 10), user.BirthDate);
        Assert.Equal("0170555666", user.PhoneNumber);
        Assert.Equal("Neuestraße", user.Street);
        Assert.Equal("7b", user.HouseNumber);
        Assert.Equal("20095", user.ZipCode);
        Assert.True(user.UpdatedAtDateTime >= originalUpdatedAt);
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateUserEntity_ShouldUpdatePassword_WhenPasswordsMatch()
    {
        // Arrange
        var user = UserTestData.CreateUser();
        var updateDto = new UserUpdateDto(
            FirstName: null,
            LastName: null,
            Email: null,
            BirthDate: null,
            PhoneNumber: null,
            Street: null,
            HouseNumber: null,
            ZipCode: null,
            Password: "NewSecretPassword1!",
            ConfirmPassword: "NewSecretPassword1!");

        // Act
        _mapper.UpdateUserEntity(user, updateDto);

        // Assert
        Assert.True(BCrypt.Net.BCrypt.Verify("NewSecretPassword1!", user.PasswordHash));
    }

    [Fact]
    [Trait("Action", "Mapping")]
    public void UpdateUserEntity_ShouldThrowArgumentException_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var user = UserTestData.CreateUser();
        var updateDto = new UserUpdateDto(
            FirstName: null,
            LastName: null,
            Email: null,
            BirthDate: null,
            PhoneNumber: null,
            Street: null,
            HouseNumber: null,
            ZipCode: null,
            Password: "Password1!",
            ConfirmPassword: "DifferentPassword2!");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _mapper.UpdateUserEntity(user, updateDto));
        Assert.Equal("Password and Confirm Password do not match.", ex.Message);
    }

    #endregion

    #region AnonymizeUserEntity Tests

    [Fact]
    [Trait("Action", "Anonymize")]
    public void AnonymizeUserEntity_ShouldOverwriteSensitiveData()
    {
        // Arrange
        var user = UserTestData.CreateUser();
        var originalId = user.Id;

        // Act
        _mapper.AnonymizeUserEntity(user);

        // Assert
        Assert.NotEqual(originalId, user.Id);
        Assert.Equal("anonymized_user", user.Role);
        Assert.Equal("null", user.FirstName);
        Assert.Equal("null", user.LastName);
        Assert.Equal("null", user.Street);
        Assert.Equal("null", user.HouseNumber);
        Assert.Null(user.PhoneNumber);
        Assert.StartsWith("anonymized_", user.Email);
        Assert.EndsWith("@system.local", user.Email);
        Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
    }

    #endregion
}