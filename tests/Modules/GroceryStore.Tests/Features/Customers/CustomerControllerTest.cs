using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Customers;
using GroceryStore.Features.Customers.Interfaces;

using GroceryStore.Tests.TestData;
using SharedKernel.Security.Interfaces;

namespace GroceryStore.Tests.Features.Customers;

[Trait("Category", "Controller")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Customers")]
public class CustomerControllerTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<ICustomerMapper> _mapperMock;
    private readonly Mock<ILogger<CustomerService>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly CustomerService _service;
    private readonly CustomerController _controller;

    public CustomerControllerTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;

        _mapperMock = new Mock<ICustomerMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<CustomerService>>();
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);
        _tokenServiceMock = new Mock<ITokenService>(MockBehavior.Strict);

        _service = new CustomerService(_context, _mapperMock.Object, _loggerMock.Object);
        _controller = new CustomerController(_service, _currentUserMock.Object, _tokenServiceMock.Object);
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
    public async Task GetCustomers_ShouldReturnOkWithCustomersList()
    {
        // Arrange
        var user = CustomerTestData.CreateCustomer();

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        var expectedDto = CustomerTestData.CreateCustomerDto(user.Id);
        _mapperMock.Setup(m => m.ToCustomerDto(It.IsAny<Customer>())).Returns(expectedDto);

        // Act
        var result = await _controller.GetCustomers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var list = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCustomers_ShouldReturnOkWithEmptyCollection_WhenNoCustomersExist()
    {
        // Act
        var result = await _controller.GetCustomers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var customers = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(okResult.Value);
        Assert.Empty(customers);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCurrentCustomer_ShouldReturnOk_WhenCustomerExists()
    {
        // Arrange
        var user = CustomerTestData.CreateCustomer();

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);

        var expectedDto = CustomerTestData.CreateCustomerDto(user.Id);
        _mapperMock.Setup(m => m.ToCustomerDto(It.Is<Customer>(u => u.Id == user.Id))).Returns(expectedDto);

        // Act
        var result = await _controller.GetCurrentCustomer();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedDto, okResult.Value);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCurrentCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _controller.GetCurrentCustomer();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region CreateUser Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateCustomer_ShouldReturnCreatedAtAction_WhenValid()
    {
        // Arrange
        var registrationDto = CustomerTestData.CreateCustomerRegistrationDto();
        var entity = CustomerTestData.CreateCustomer(email: registrationDto.Email, zipCode: registrationDto.ZipCode);
        var createdDto = CustomerTestData.CreateCustomerDto(entity.Id, email: registrationDto.Email);

        _mapperMock.Setup(m => m.ToCustomerEntity(registrationDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToCustomerDto(entity)).Returns(createdDto);

        // Act
        var result = await _controller.CreateCustomer(registrationDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
        Assert.Equal(nameof(CustomerController.GetCurrentCustomer), createdAtResult.ActionName);
        Assert.Same(createdDto, createdAtResult.Value);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateCustomer_ShouldReturnBadRequest_WhenEmailOrPhoneInUse()
    {
        // Arrange
        var existing = CustomerTestData.CreateCustomer(email: "used@domain.de");

        _context.Customers.Add(existing);
        await _context.SaveChangesAsync();

        var registrationDto = CustomerTestData.CreateCustomerRegistrationDto(email: "used@domain.de");
        var entity = CustomerTestData.CreateCustomer(email: "used@domain.de");

        _mapperMock.Setup(m => m.ToCustomerEntity(registrationDto)).Returns(entity);

        // Act
        var result = await _controller.CreateCustomer(registrationDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Email is already in use.", badRequestResult.Value);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateCustomer_ShouldReturnBadRequest_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var registrationDto = CustomerTestData.CreateCustomerRegistrationDto(
            password: "SecurePassword123!",
            confirmPassword: "DifferentPassword123!");

        // Act
        var result = await _controller.CreateCustomer(registrationDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Passwords do not match.", badRequestResult.Value);
    }

    #endregion

    #region LoginUser Tests

    [Fact]
    [Trait("Action", "Login")]
    public async Task LoginCustomer_ShouldReturnOkWithToken_WhenCredentialsAreValid()
    {
        // Arrange
        const string rawPassword = "CorrectPass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var user = CustomerTestData.CreateCustomer(
            email: "login@domain.de",
            passwordHash: passwordHash);

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        var userDto = CustomerTestData.CreateCustomerDto(user.Id, email: user.Email, role: "customer");
        _mapperMock.Setup(m => m.ToCustomerDto(It.Is<Customer>(u => u.Id == user.Id))).Returns(userDto);
        _tokenServiceMock.Setup(t => t.GenerateToken(user.Id, "login@domain.de", "customer", null))
            .Returns("generated_user_jwt_token");

        var loginDto = new CustomerLoginDto("login@domain.de", rawPassword);

        // Act
        var result = await _controller.LoginCustomer(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var authResponse = Assert.IsType<CustomerAuthResponseDto>(okResult.Value);
        Assert.Equal("generated_user_jwt_token", authResponse.Token);
        Assert.Equal(user.Id, authResponse.CustomerId);
        Assert.Equal("customer", authResponse.Role);
    }

    [Fact]
    [Trait("Action", "Login")]
    public async Task LoginCustomer_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var loginDto = new CustomerLoginDto("nonexistent@domain.de", "WrongPass");

        // Act
        var result = await _controller.LoginCustomer(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        Assert.Equal("Invalid email or password.", unauthorizedResult.Value);
    }

    #endregion

    #region UpdateUser Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCustomer_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var user = CustomerTestData.CreateCustomer();

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);
        var updateDto = CustomerTestData.CreateCustomerUpdateDto(email: "newemail@domain.de");
        _mapperMock.Setup(m => m.UpdateCustomerEntity(user, updateDto))
            .Callback<Customer, CustomerUpdateDto>((customer, dto) => customer.Email = dto.Email!);

        // Act
        var result = await _controller.UpdateCustomer(updateDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        Assert.Equal("newemail@domain.de", (await _context.Customers.FindAsync(user.Id))!.Email);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCustomer_ShouldReturnNotFound_WhenCustomerMissing()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        var updateDto = CustomerTestData.CreateCustomerUpdateDto();

        // Act
        var result = await _controller.UpdateCustomer(updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCustomer_ShouldReturnBadRequest_WhenEmailIsAlreadyInUse()
    {
        // Arrange
        var currentCustomer = CustomerTestData.CreateCustomer(email: "current@domain.de");
        var existingCustomer = CustomerTestData.CreateCustomer(email: "used@domain.de");
        _context.Customers.AddRange(currentCustomer, existingCustomer);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(currentCustomer.Id);
        var updateDto = CustomerTestData.CreateCustomerUpdateDto(email: existingCustomer.Email);

        // Act
        var result = await _controller.UpdateCustomer(updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Email is already in use.", badRequestResult.Value);
    }

    #endregion

    #region DeleteUser Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCustomer_ShouldReturnNoContent_WhenPasswordIsCorrect()
    {
        // Arrange
        const string rawPassword = "CorrectPass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var user = CustomerTestData.CreateCustomer(passwordHash: passwordHash);

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);
        _mapperMock.Setup(m => m.AnonymizeCustomerEntity(user))
            .Callback<Customer>(customer => customer.Role = "anonymized_customer");

        // Act
        var result = await _controller.DeleteCustomer(new CustomerActionRequest(rawPassword));

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        Assert.Equal("anonymized_customer", (await _context.Customers.FindAsync(user.Id))!.Role);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCustomer_ShouldReturnBadRequest_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = CustomerTestData.CreateCustomer(
            passwordHash: BCrypt.Net.BCrypt.HashPassword("CorrectPassword"));

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(user.Id);

        // Act
        var result = await _controller.DeleteCustomer(new CustomerActionRequest("WrongPassword"));

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid password.", badRequestResult.Value);
    }

    #endregion
}