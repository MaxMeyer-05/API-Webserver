using Moq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Categories;
using GroceryStore.Features.Categories.Interfaces;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Categories;

[Trait("Category", "Service")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Categories")]
public class CategoryServiceTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<ICategoryMapper> _mapperMock;
    private readonly Mock<ILogger<CategoryService>> _loggerMock;
    private readonly CategoryService _service;

    public CategoryServiceTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;
        _mapperMock = new Mock<ICategoryMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<CategoryService>>();

        _service = new CategoryService(_context, _mapperMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region CreateCategoryAsync Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateCategoryAsync_ShouldPersistCategoryAndReturnDto()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = CategoryTestData.CreateCategoryCreateDto("Frühstück", supplier.Id, []);
        var entityToInsert = new Category { Name = "Frühstück", SupplierId = supplier.Id };
        var expectedDto = CategoryTestData.CreateCategoryDto("Frühstück", supplier.Id);

        _mapperMock.Setup(m => m.ToCategoryEntity(createDto)).Returns(entityToInsert);
        _mapperMock.Setup(m => m.ToCategoryDto(It.Is<Category>(c => c.Name == "Frühstück"))).Returns(expectedDto);

        // Act
        var result = await _service.CreateCategoryAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Frühstück", result.Name);
        Assert.Equal(supplier.Id, result.SupplierId);

        var persisted = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Frühstück");
        Assert.NotNull(persisted);
        Assert.NotEqual(0, persisted.Id);

        _mapperMock.Verify(m => m.ToCategoryEntity(createDto), Times.Once);
        _mapperMock.Verify(m => m.ToCategoryDto(entityToInsert), Times.Once);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateCategoryAsync_ShouldThrowInvalidOperationException_WhenDtoMappingFails()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = CategoryTestData.CreateCategoryCreateDto("Snacks", supplier.Id, []);
        var entity = new Category { Name = "Snacks", SupplierId = supplier.Id };

        _mapperMock.Setup(m => m.ToCategoryEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToCategoryDto(entity)).Returns((CategoryDto)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateCategoryAsync(createDto));
        Assert.Equal("Failed to map the created category entity to DTO.", ex.Message);
    }

    #endregion

    #region GetAllCategoriesAsync & GetCategoryByIdAsync Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllCategoriesAsync_ShouldReturnAllMappedCategories()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var category1 = CategoryTestData.CreateCategory(0, "Frühstück", supplier.Id);
        var category2 = CategoryTestData.CreateCategory(0, "Mittagessen", supplier.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.ToCategoryDto(It.Is<Category>(c => c.Name == "Frühstück")))
            .Returns(CategoryTestData.CreateCategoryDto("Frühstück", supplier.Id));
        _mapperMock.Setup(m => m.ToCategoryDto(It.Is<Category>(c => c.Name == "Mittagessen")))
            .Returns(CategoryTestData.CreateCategoryDto("Mittagessen", supplier.Id));

        // Act
        var result = await _service.GetAllCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, c => c.Name == "Frühstück");
        Assert.Contains(list, c => c.Name == "Mittagessen");
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCategoryByIdAsync_ShouldReturnMappedDto_WhenCategoryExists()
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
        var result = await _service.GetCategoryByIdAsync(category.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Desserts", result.Name);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetCategoryByIdAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetCategoryByIdAsync(999));
        Assert.Contains("999", ex.Message);
    }

    #endregion

    #region UpdateCategoryAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCategoryAsync_ShouldApplyUpdates_WhenSupplierIsOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var category = CategoryTestData.CreateCategory(0, "Alt", supplier.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var updateDto = CategoryTestData.CreateCategoryUpdateDto("Aktualisiert");
        _mapperMock.Setup(m => m.UpdateCategoryEntity(category, updateDto))
            .Callback<Category, CategoryUpdateDto>((entity, dto) => entity.Name = dto.Name!);

        // Act
        await _service.UpdateCategoryAsync(category.Id, supplier.Id, updateDto);

        // Assert
        var updated = await _context.Categories.FindAsync(category.Id);
        Assert.NotNull(updated);
        Assert.Equal("Aktualisiert", updated.Name);
        _mapperMock.Verify(m => m.UpdateCategoryEntity(category, updateDto), Times.Once);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCategoryAsync_ShouldThrowUnauthorizedAccessException_WhenSupplierIsNotOwner()
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

        var updateDto = CategoryTestData.CreateCategoryUpdateDto("Neu");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateCategoryAsync(category.Id, strangerId, updateDto));
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateCategoryAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var updateDto = CategoryTestData.CreateCategoryUpdateDto();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateCategoryAsync(404, Guid.NewGuid(), updateDto));
    }

    #endregion

    #region DeleteCategoryAsync Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCategoryAsync_ShouldRemoveCategory_WhenSupplierIsOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var supplier = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var category = CategoryTestData.CreateCategory(0, "Suppen", supplier.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(supplier);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteCategoryAsync(category.Id, supplier.Id);

        // Assert
        var exists = await _context.Categories.AnyAsync(c => c.Id == category.Id);
        Assert.False(exists);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCategoryAsync_ShouldThrowUnauthorizedAccessException_WhenSupplierIsNotOwner()
    {
        // Arrange
        var location = GroceryStoreTestData.CreateLocation();
        var owner = GroceryStoreTestData.CreateSupplier(zipCode: location.ZipCode);
        var category = CategoryTestData.CreateCategory(0, "Snacks", owner.Id);

        _context.Locations.Add(location);
        _context.Suppliers.Add(owner);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeleteCategoryAsync(category.Id, Guid.NewGuid()));
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteCategoryAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.DeleteCategoryAsync(999, Guid.NewGuid()));
    }

    #endregion
}