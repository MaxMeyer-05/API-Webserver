using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Categories;
using GroceryStore.Features.Categories.Interfaces;

using GroceryStore.Tests.TestData;
using SharedKernel.Security.Interfaces;

namespace GroceryStore.Tests.Features.Categories;

[Trait("Category", "Controller")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Categories")]
public class CategoryControllerTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<ICategoryMapper> _mapperMock;
    private readonly Mock<ILogger<CategoryService>> _loggerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly CategoryService _service;
    private readonly CategoryController _controller;

    public CategoryControllerTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;

        _mapperMock = new Mock<ICategoryMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<CategoryService>>();
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);

        _service = new CategoryService(_context, _mapperMock.Object, _loggerMock.Object);
        _controller = new CategoryController(_service, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetAllCategories Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllCategories_ShouldReturnOkWithCategories()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var category = CategoryTestData.CreateCategory(0, "Frühstück", supplier.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var expectedDto = CategoryTestData.CreateCategoryDto("Frühstück", supplier.Id);
        _mapperMock.Setup(m => m.ToCategoryDto(It.IsAny<Category>())).Returns(expectedDto);

        // Act
        var result = await _controller.GetAllCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
        Assert.Single(returnedItems);
    }

    #endregion

    #region GetCategoryById Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCategoryById_ShouldReturnOk_WhenCategoryExists()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var category = CategoryTestData.CreateCategory(0, "Desserts", supplier.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var expectedDto = CategoryTestData.CreateCategoryDto("Desserts", supplier.Id);
        _mapperMock.Setup(m => m.ToCategoryDto(It.Is<Category>(c => c.Id == category.Id))).Returns(expectedDto);

        // Act
        var result = await _controller.GetCategoryById(category.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedDto, okResult.Value);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCategoryById_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Act
        var result = await _controller.GetCategoryById(404);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    #endregion

    #region CreateCategory Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateCategory_ShouldReturnCreatedAtAction_WhenValid()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = CategoryTestData.CreateCategoryCreateDto("Backwaren", supplier.Id);
        var entity = new Category { Name = "Backwaren", SupplierId = supplier.Id };
        var createdDto = CategoryTestData.CreateCategoryDto("Backwaren", supplier.Id);

        _mapperMock.Setup(m => m.ToCategoryEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToCategoryDto(It.Is<Category>(c => c.Name == "Backwaren"))).Returns(createdDto);

        // Act
        var result = await _controller.CreateCategory(createDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, createdAtResult.StatusCode);
        Assert.Equal(nameof(CategoryController.GetCategoryById), createdAtResult.ActionName);
        Assert.Same(createdDto, createdAtResult.Value);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenMappingFails()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = CategoryTestData.CreateCategoryCreateDto(supplierId: supplier.Id);
        var entity = new Category { Name = createDto.Name, SupplierId = createDto.SupplierId };

        _mapperMock.Setup(m => m.ToCategoryEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToCategoryDto(It.IsAny<Category>())).Returns((CategoryDto)null!);

        // Act
        var result = await _controller.CreateCategory(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    #endregion

    #region UpdateCategory Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCategory_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var category = CategoryTestData.CreateCategory(0, "Alt", supplier.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var updateDto = CategoryTestData.CreateCategoryUpdateDto("Neu");
        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);
        _mapperMock.Setup(m => m.UpdateCategoryEntity(category, updateDto))
            .Callback<Category, CategoryUpdateDto>((entity, dto) => entity.Name = dto.Name!);

        // Act
        var result = await _controller.UpdateCategory(category.Id, updateDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCategory_ShouldReturnNotFound_WhenCategoryMissing()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        var updateDto = CategoryTestData.CreateCategoryUpdateDto();

        // Act
        var result = await _controller.UpdateCategory(999, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCategory_ShouldReturnForbid_WhenSupplierIsNotOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var owner = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var strangerId = Guid.NewGuid();
        var category = CategoryTestData.CreateCategory(0, "Frühstück", owner.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(owner);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(strangerId);
        var updateDto = CategoryTestData.CreateCategoryUpdateDto();

        // Act
        var result = await _controller.UpdateCategory(category.Id, updateDto);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.NotNull(forbidResult);
    }

    #endregion

    #region DeleteCategory Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCategory_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var category = CategoryTestData.CreateCategory(0, "Suppen", supplier.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(supplier.Id);

        // Act
        var result = await _controller.DeleteCategory(category.Id);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCategory_ShouldReturnNotFound_WhenCategoryMissing()
    {
        // Arrange
        _currentUserMock.SetupGet(u => u.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _controller.DeleteCategory(500);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCategory_ShouldReturnForbid_WhenSupplierIsNotOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var owner = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var strangerId = Guid.NewGuid();
        var category = CategoryTestData.CreateCategory(0, "Getränke", owner.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(owner);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _currentUserMock.SetupGet(u => u.UserId).Returns(strangerId);

        // Act
        var result = await _controller.DeleteCategory(category.Id);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.NotNull(forbidResult);
    }

    #endregion
}