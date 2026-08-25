using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Users;
using GroceryStore.Features.Users.Interfaces;

using GroceryStore.Tests.TestData;
using SharedKernel.Security.Interfaces;

namespace GroceryStore.Tests.Features.Users;

[Trait("Category", "Controller")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Users")]
public class UserControllerTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<IUserMapper> _mapperMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly UserService _service;
    private readonly UserController _controller;

    public UserControllerTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;

        _mapperMock = new Mock<IUserMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<UserService>>();
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);
        _tokenServiceMock = new Mock<ITokenService>(MockBehavior.Strict);

        _service = new UserService(_context, _mapperMock.Object, _loggerMock.Object);
        _controller = new UserController(_service, _currentUserMock.Object, _tokenServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetUsers & GetCurrentUser Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetUsers_ShouldReturnOkWithUsersList()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = UserTestData.CreateUser(zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var expectedDto = UserTestData.CreateUserDto(user.Id);
        _mapperMock.Setup(m => m.ToUserDto(It.IsAny<User>())).Returns(expectedDto);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var list = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCurrentUser_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = UserTestData.CreateUser(zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);

        var expectedDto = UserTestData.CreateUserDto(user.Id);
        _mapperMock.Setup(m => m.ToUserDto(It.Is<User>(u => u.Id == user.Id))).Returns(expectedDto);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedDto, okResult.Value);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCurrentUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region CreateUser Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateUser_ShouldReturnCreatedAtAction_WhenValid()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var registrationDto = UserTestData.CreateUserRegistrationDto();
        var entity = UserTestData.CreateUser(email: registrationDto.Email, zipCode: location.ZipCode);
        var createdDto = UserTestData.CreateUserDto(entity.Id, email: registrationDto.Email);

        _mapperMock.Setup(m => m.ToUserEntity(registrationDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToUserDto(entity)).Returns(createdDto);

        // Act
        var result = await _controller.CreateUser(registrationDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
        Assert.Equal(nameof(UserController.CreateUser), createdAtResult.ActionName);
        Assert.Same(createdDto, createdAtResult.Value);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateUser_ShouldReturnBadRequest_WhenEmailOrPhoneInUse()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var existing = UserTestData.CreateUser(email: "used@domain.de", zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(existing);
        await _context.SaveChangesAsync();

        var registrationDto = UserTestData.CreateUserRegistrationDto(email: "used@domain.de");
        var entity = UserTestData.CreateUser(email: "used@domain.de", zipCode: location.ZipCode);

        _mapperMock.Setup(m => m.ToUserEntity(registrationDto)).Returns(entity);

        // Act
        var result = await _controller.CreateUser(registrationDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Email is already in use.", badRequestResult.Value);
    }

    #endregion

    #region LoginUser Tests

    [Fact]
    [Trait("Action", "Login")]
    public async Task LoginUser_ShouldReturnOkWithToken_WhenCredentialsAreValid()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        const string rawPassword = "CorrectPass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var user = UserTestData.CreateUser(
            email: "login@domain.de",
            passwordHash: passwordHash,
            zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var userDto = UserTestData.CreateUserDto(user.Id, email: user.Email, role: "user");
        _mapperMock.Setup(m => m.ToUserDto(It.Is<User>(u => u.Id == user.Id))).Returns(userDto);
        _tokenServiceMock.Setup(t => t.GenerateToken(user.Id, "login@domain.de", "user", null))
            .Returns("generated_user_jwt_token");

        var loginDto = new UserLoginDto("login@domain.de", rawPassword);

        // Act
        var result = await _controller.LoginUser(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var authResponse = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("generated_user_jwt_token", authResponse.Token);
        Assert.Equal(user.Id, authResponse.UserId);
        Assert.Equal("user", authResponse.Role);
    }

    [Fact]
    [Trait("Action", "Login")]
    public async Task LoginUser_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var loginDto = new UserLoginDto("nonexistent@domain.de", "WrongPass");

        // Act
        var result = await _controller.LoginUser(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        Assert.Equal("Invalid email or password.", unauthorizedResult.Value);
    }

    #endregion

    #region UpdateUser Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateUser_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = UserTestData.CreateUser(zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);
        var updateDto = UserTestData.CreateUserUpdateDto(email: "newemail@domain.de");
        _mapperMock.Setup(m => m.UpdateUserEntity(user, updateDto));

        // Act
        var result = await _controller.UpdateUser(updateDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserMissing()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        var updateDto = UserTestData.CreateUserUpdateDto();

        // Act
        var result = await _controller.UpdateUser(updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region DeleteUser Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteUser_ShouldReturnNoContent_WhenPasswordIsCorrect()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        const string rawPassword = "CorrectPass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var user = UserTestData.CreateUser(passwordHash: passwordHash, zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);
        _mapperMock.Setup(m => m.AnonymizeUserEntity(user));

        // Act
        var result = await _controller.DeleteUser(rawPassword);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenPasswordIsIncorrect()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var user = UserTestData.CreateUser(
            passwordHash: BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            zipCode: location.ZipCode);

        _context.Locations.Add(location);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);

        // Act
        var result = await _controller.DeleteUser("WrongPassword");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid password.", badRequestResult.Value);
    }

    #endregion
}