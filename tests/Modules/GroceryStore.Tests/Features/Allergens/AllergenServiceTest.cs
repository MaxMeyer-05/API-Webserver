using Moq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

using GroceryStore.Features.Allergens;
using GroceryStore.Features.Allergens.Interfaces;

using GroceryStore.Tests.TestData;

namespace GroceryStore.Tests.Features.Allergens;

[Trait("Category", "Service")]
[Trait("Module", "GroceryStore")]
[Trait("Feature", "Allergens")]
public class AllergenServiceTest : IDisposable
{
    private readonly GroceryStoreDbContext _context;
    private readonly IDisposable _connection;
    private readonly Mock<IAllergenMapper> _mapperMock;
    private readonly Mock<ILogger<AllergenService>> _loggerMock;
    private readonly AllergenService _service;

    public AllergenServiceTest()
    {
        var (context, connection) = GroceryStoreTestData.CreateInMemoryDbContext();
        _context = context;
        _connection = connection;
        _mapperMock = new Mock<IAllergenMapper>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<AllergenService>>();

        _service = new AllergenService(_context, _mapperMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region CreateAllergenAsync Tests

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateAllergenAsync_ShouldPersistAllergenAndReturnDto()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = AllergenTestData.CreateAllergenCreateDto("Gluten", supplier.Id, []);
        var entityToInsert = new Allergen { Name = "Gluten", SupplierId = supplier.Id };
        var expectedDto = AllergenTestData.CreateAllergenDto("Gluten", supplier.Id);

        _mapperMock.Setup(m => m.ToAllergenEntity(createDto)).Returns(entityToInsert);
        _mapperMock.Setup(m => m.ToAllergenDto(It.Is<Allergen>(a => a.Name == "Gluten"))).Returns(expectedDto);

        // Act
        var result = await _service.CreateAllergenAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Gluten", result.Name);
        Assert.Equal(supplier.Id, result.SupplierId);

        var persisted = await _context.Allergens.FirstOrDefaultAsync(a => a.Name == "Gluten");
        Assert.NotNull(persisted);
        Assert.NotEqual(0, persisted.Id);

        _mapperMock.Verify(m => m.ToAllergenEntity(createDto), Times.Once);
        _mapperMock.Verify(m => m.ToAllergenDto(entityToInsert), Times.Once);
    }

    [Fact]
    [Trait("Action", "Create")]
    public async Task CreateAllergenAsync_ShouldThrowInvalidOperationException_WhenDtoMappingFails()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var createDto = AllergenTestData.CreateAllergenCreateDto("Nüsse", supplier.Id, []);
        var entity = new Allergen { Name = "Nüsse", SupplierId = supplier.Id };

        _mapperMock.Setup(m => m.ToAllergenEntity(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.ToAllergenDto(entity)).Returns((AllergenDto)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAllergenAsync(createDto));
        Assert.Equal("Failed to map the created allergen entity to DTO", ex.Message);
    }

    #endregion

    #region GetAllAllergensAsync & GetAllergenByIdAsync Tests

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllAllergensAsync_ShouldReturnAllMappedAllergens()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen1 = AllergenTestData.CreateAllergen(0, "Gluten", supplier.Id);
        var allergen2 = AllergenTestData.CreateAllergen(0, "Laktose", supplier.Id);

        _context.Suppliers.Add(supplier);
        _context.Allergens.AddRange(allergen1, allergen2);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.ToAllergenDto(It.Is<Allergen>(a => a.Name == "Gluten")))
            .Returns(AllergenTestData.CreateAllergenDto("Gluten", supplier.Id));
        _mapperMock.Setup(m => m.ToAllergenDto(It.Is<Allergen>(a => a.Name == "Laktose")))
            .Returns(AllergenTestData.CreateAllergenDto("Laktose", supplier.Id));

        // Act
        var result = await _service.GetAllAllergensAsync();

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, a => a.Name == "Gluten");
        Assert.Contains(list, a => a.Name == "Laktose");
        _mapperMock.Verify(m => m.ToAllergenDto(allergen1), Times.Once);
        _mapperMock.Verify(m => m.ToAllergenDto(allergen2), Times.Once);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllAllergensAsync_ShouldReturnEmptyCollection_WhenNoAllergensExist()
    {
        // Act
        var result = await _service.GetAllAllergensAsync();

        // Assert
        Assert.Empty(result);
        _mapperMock.Verify(m => m.ToAllergenDto(It.IsAny<Allergen>()), Times.Never);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllergenByIdAsync_ShouldReturnMappedDto_WhenAllergenExists()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen = AllergenTestData.CreateAllergen(0, "Sellerie", supplier.Id);

        _context.Suppliers.Add(supplier);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        var expectedDto = AllergenTestData.CreateAllergenDto("Sellerie", supplier.Id);
        _mapperMock.Setup(m => m.ToAllergenDto(It.Is<Allergen>(a => a.Id == allergen.Id))).Returns(expectedDto);

        // Act
        var result = await _service.GetAllergenByIdAsync(allergen.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Sellerie", result.Name);
        _mapperMock.Verify(m => m.ToAllergenDto(allergen), Times.Once);
    }

    [Fact]
    [Trait("Action", "Get")]
    public async Task GetAllergenByIdAsync_ShouldThrowKeyNotFoundException_WhenAllergenDoesNotExist()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAllergenByIdAsync(999));
        Assert.Contains("999", ex.Message);
    }

    #endregion

    #region UpdateAllergenAsync Tests

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateAllergenAsync_ShouldApplyUpdates_WhenSupplierIsOwner()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen = AllergenTestData.CreateAllergen(0, "Alt", supplier.Id);

        _context.Suppliers.Add(supplier);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        var updateDto = AllergenTestData.CreateAllergenUpdateDto("Aktualisiert");
        _mapperMock.Setup(m => m.UpdateAllergenEntity(allergen, updateDto))
            .Callback<Allergen, AllergenUpdateDto>((entity, dto) => entity.Name = dto.Name!);

        // Act
        await _service.UpdateAllergenAsync(allergen.Id, supplier.Id, updateDto);

        // Assert
        var updated = await _context.Allergens.FindAsync(allergen.Id);
        Assert.NotNull(updated);
        Assert.Equal("Aktualisiert", updated.Name);
        _mapperMock.Verify(m => m.UpdateAllergenEntity(allergen, updateDto), Times.Once);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateAllergenAsync_ShouldThrowUnauthorizedAccessException_WhenSupplierIsNotOwner()
    {
        // Arrange
        var owner = GroceryStoreTestData.CreateSupplier();
        var strangerId = Guid.NewGuid();
        var allergen = AllergenTestData.CreateAllergen(0, "Gluten", owner.Id);

        _context.Suppliers.Add(owner);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        var updateDto = AllergenTestData.CreateAllergenUpdateDto("Neu");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateAllergenAsync(allergen.Id, strangerId, updateDto));
        Assert.Equal("Gluten", (await _context.Allergens.FindAsync(allergen.Id))!.Name);
        _mapperMock.Verify(m => m.UpdateAllergenEntity(It.IsAny<Allergen>(), It.IsAny<AllergenUpdateDto>()), Times.Never);
    }

    [Fact]
    [Trait("Action", "Update")]
    public async Task UpdateAllergenAsync_ShouldThrowKeyNotFoundException_WhenAllergenDoesNotExist()
    {
        // Arrange
        var updateDto = AllergenTestData.CreateAllergenUpdateDto();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAllergenAsync(404, Guid.NewGuid(), updateDto));
    }

    #endregion

    #region DeleteAllergenAsync Tests

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteAllergenAsync_ShouldRemoveAllergen_WhenSupplierIsOwner()
    {
        // Arrange
        var supplier = GroceryStoreTestData.CreateSupplier();
        var allergen = AllergenTestData.CreateAllergen(0, "Senf", supplier.Id);

        _context.Suppliers.Add(supplier);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAllergenAsync(allergen.Id, supplier.Id);

        // Assert
        var exists = await _context.Allergens.AnyAsync(a => a.Id == allergen.Id);
        Assert.False(exists);
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteAllergenAsync_ShouldThrowUnauthorizedAccessException_WhenSupplierIsNotOwner()
    {
        // Arrange
        var owner = GroceryStoreTestData.CreateSupplier();
        var allergen = AllergenTestData.CreateAllergen(0, "Sesam", owner.Id);

        _context.Suppliers.Add(owner);
        _context.Allergens.Add(allergen);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeleteAllergenAsync(allergen.Id, Guid.NewGuid()));
        Assert.NotNull(await _context.Allergens.FindAsync(allergen.Id));
    }

    [Fact]
    [Trait("Action", "Delete")]
    public async Task DeleteAllergenAsync_ShouldThrowKeyNotFoundException_WhenAllergenDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.DeleteAllergenAsync(999, Guid.NewGuid()));
    }

    #endregion
}