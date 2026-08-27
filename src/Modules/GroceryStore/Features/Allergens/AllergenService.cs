using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Features.Allergens.Interfaces;

namespace GroceryStore.Features.Allergens;

/// <summary>
/// Represents the service for managing allergens in the grocery store module.
/// </summary>
public class AllergenService : IAllergenService
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly IAllergenMapper _allergenMapper;
    private readonly ILogger<AllergenService> _logger;

    public AllergenService(
        GroceryStoreDbContext dbContext,
        IAllergenMapper allergenMapper,
        ILogger<AllergenService> logger)
    {
        _dbContext = dbContext;
        _allergenMapper = allergenMapper;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<AllergenDto> CreateAllergenAsync(AllergenCreateDto allergen)
    {
        var allergenEntity = _allergenMapper.ToAllergenEntity(allergen);

        _dbContext.Allergens.Add(allergenEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new allergen with ID {AllergenId}", allergenEntity.Id);

        return _allergenMapper.ToAllergenDto(allergenEntity)
            ?? throw new InvalidOperationException("Failed to map the created allergen entity to DTO");
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task DeleteAllergenAsync(int allergenId, Guid supplierId)
    {
        var allergenEntity = await _dbContext.Allergens
            .Where(a => a.Id == allergenId)
            .FirstOrDefaultAsync() 
            ?? throw new KeyNotFoundException($"Allergen with ID {allergenId} not found");

        if (allergenEntity.SupplierId != supplierId)
            throw new UnauthorizedAccessException($"Supplier with ID {supplierId} is not authorized to delete allergen with ID {allergenId}");

        _dbContext.Allergens.Remove(allergenEntity);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Deleted allergen with ID {AllergenId}", allergenId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AllergenDto>> GetAllAllergensAsync()
    {
        var allergens = await _dbContext.Allergens
            .Include(a => a.Ingredients)
            .ToListAsync();

        return allergens.Select(a => _allergenMapper.ToAllergenDto(a));
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<AllergenDto?> GetAllergenByIdAsync(int allergenId)
    {
        var allergen = await _dbContext.Allergens
            .Include(a => a.Ingredients)
            .Where(a => a.Id == allergenId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Allergen with ID {allergenId} not found");

        return _allergenMapper.ToAllergenDto(allergen);
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task UpdateAllergenAsync(int allergenId, Guid supplierId, AllergenUpdateDto allergen)
    {
        var allergenEntity = await _dbContext.Allergens.FindAsync(allergenId) 
            ?? throw new KeyNotFoundException($"Allergen with ID {allergenId} not found");

        if (allergenEntity.SupplierId != supplierId)
            throw new UnauthorizedAccessException($"Supplier with ID {supplierId} is not authorized to update allergen with ID {allergenId}");

        _allergenMapper.UpdateAllergenEntity(allergenEntity, allergen);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated allergen with ID {AllergenId}", allergenId);
    }
}