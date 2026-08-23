using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Features.Suppliers.Interfaces;

namespace GroceryStore.Features.Suppliers;

/// <summary>
/// Repository class for managing supplier data in the database.
/// </summary>
public class SupplierRepository : ISupplierRepository
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly ISupplierMapper _supplierMapper;
    private readonly ILogger<SupplierRepository> _logger;

    public SupplierRepository(
        GroceryStoreDbContext dbContext, 
        ISupplierMapper supplierMapper,
        ILogger<SupplierRepository> logger)
    {
        _dbContext = dbContext;
        _supplierMapper = supplierMapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SupplierDto> CreateSupplierAsync(SupplierRegistrationDto supplier)
    {
        var supplierEntity = _supplierMapper.ToSupplierEntity(supplier);
        await _dbContext.Suppliers.AddAsync(supplierEntity);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Created new supplier with Id {SupplierId}", supplierEntity.Id);
        return _supplierMapper.ToSupplierDto(supplierEntity);
    }

    /// <inheritdoc />
    public async Task DeleteSupplierAsync(Guid supplierId)
    {
        var supplier = await _dbContext.Suppliers
            .Where(s => s.Id == supplierId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Supplier with Id {supplierId} not found");

        _supplierMapper.AnonymizeSupplierEntity(supplier);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Anonymized supplier with Id {SupplierId}", supplierId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
    {
        var suppliers = await _dbContext.Suppliers.ToListAsync();
        return suppliers.Select(s => _supplierMapper.ToSupplierDto(s));
    }

    /// <inheritdoc />
    public async Task<SupplierDto?> GetSupplierByIdAsync(Guid supplierId)
    {
        var supplier = await _dbContext.Suppliers
            .Where(s => s.Id == supplierId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Supplier with Id {supplierId} not found");

        return _supplierMapper.ToSupplierDto(supplier);
    }

    /// <inheritdoc />
    public async Task<SupplierDto?> LoginSupplierAsync(SupplierLoginDto supplier)
    {
        var passwordHash = HashPassword(supplier.Password);
        var existingSupplier = await _dbContext.Suppliers
            .Where(s => s.Email == supplier.Email && s.PasswordHash == passwordHash)
            .FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("Invalid email or password");

        _logger.LogDebug("Supplier with email {Email} logged in successfully", supplier.Email);
        return _supplierMapper.ToSupplierDto(existingSupplier);
    }

    /// <inheritdoc />
    public async Task UpdateSupplierAsync(Guid supplierId, SupplierUpdateDto supplier)
    {
        var existingSupplier = await _dbContext.Suppliers
            .Where(s => s.Id == supplierId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Supplier with Id {supplierId} not found");

        _supplierMapper.UpdateSupplierEntity(existingSupplier, supplier);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated supplier with Id {SupplierId}", supplierId);
    }

    /// <summary>
    /// Hashes the provided password using BCrypt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password.</returns>
    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}