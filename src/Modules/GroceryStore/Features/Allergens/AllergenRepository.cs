using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Features.Allergens.Interfaces;

namespace GroceryStore.Features.Allergens;

/// <summary>
/// Represents a repository for managing allergens in the database.
/// </summary>
public class AllergenRepository : IAllergenRepository
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly IAllergenMapper _allergenMapper;
    private readonly ILogger<AllergenRepository> _logger;

    public AllergenRepository(
        GroceryStoreDbContext dbContext,
        IAllergenMapper allergenMapper,
        ILogger<AllergenRepository> logger)
    {
        _dbContext = dbContext;
        _allergenMapper = allergenMapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AllergenDto> CreateAllergenAsync(AllergenCreateDto allergen)
    {
        var allergenEntity = _allergenMapper.ToAllergenEntity(allergen);

        _dbContext.Allergens.Add(allergenEntity);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new allergen with ID {AllergenId}", allergenEntity.Id);

        var createdAllergen = _allergenMapper.ToAllergenDto(allergenEntity);
        return createdAllergen;
    }

    /// <inheritdoc />
    public async Task DeleteAllergenAsync(int allergenId)
    {
        var allergenEntity = await _dbContext.Allergens.FindAsync(allergenId) 
            ?? throw new KeyNotFoundException($"Allergen with ID {allergenId} not found");
        
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

        _logger.LogDebug("Retrieved all allergens from the database: {@Allergens}", allergens);
        return allergens.Select(a => _allergenMapper.ToAllergenDto(a));
    }

    /// <inheritdoc />
    public async Task<AllergenDto?> GetAllergenByIdAsync(int allergenId)
    {
        var allergen = await _dbContext.Allergens
            .Include(a => a.Ingredients)
            .Where(a => a.Id == allergenId)
            .FirstOrDefaultAsync();

        _logger.LogDebug("Retrieved allergen with ID {AllergenId}: {@Allergen}", allergenId, allergen);
        return allergen == null ? null : _allergenMapper.ToAllergenDto(allergen);
    }

    /// <inheritdoc />
    public async Task UpdateAllergenAsync(int allergenId, AllergenUpdateDto allergen)
    {
        var allergenEntity = await _dbContext.Allergens.FindAsync(allergenId) 
            ?? throw new KeyNotFoundException($"Allergen with ID {allergenId} not found");
            
        _allergenMapper.UpdateAllergenEntity(allergenEntity, allergen);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated allergen with ID {AllergenId}", allergenId);
    }
}