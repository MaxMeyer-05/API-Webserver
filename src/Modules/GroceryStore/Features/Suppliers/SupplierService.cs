using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Features.Suppliers.Interfaces;

namespace GroceryStore.Features.Suppliers;

/// <summary>
/// Service class for managing supplier data in the database.
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly ISupplierMapper _supplierMapper;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        GroceryStoreDbContext dbContext, 
        ISupplierMapper supplierMapper,
        ILogger<SupplierService> logger)
    {
        _dbContext = dbContext;
        _supplierMapper = supplierMapper;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<SupplierDto> CreateSupplierAsync(SupplierRegistrationDto supplier)
    {
        if (supplier.Password != supplier.ConfirmPassword)
            throw new InvalidOperationException("Passwords do not match.");

        var supplierEntity = _supplierMapper.ToSupplierEntity(supplier);

        if (await IsEmailInUseAsync(supplierEntity.Email))
            throw new InvalidOperationException("Email is already in use.");

        if (supplierEntity.PhoneNumber is not null 
            && await IsPhoneNumberInUseAsync(supplierEntity.PhoneNumber))
            throw new InvalidOperationException("Phone number is already in use.");

        await _dbContext.Suppliers.AddAsync(supplierEntity);
        await _dbContext.SaveChangesAsync();
        await _dbContext.Entry(supplierEntity)
            .Reference(s => s.ZipCodeNavigation)
            .LoadAsync();
        _logger.LogInformation("Created new supplier with Id {SupplierId}", supplierEntity.Id);
        return _supplierMapper.ToSupplierDto(supplierEntity);
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task DeleteSupplierAsync(Guid supplierId, string password)
    {
        var supplier = await _dbContext.Suppliers
            .Where(s => s.Id == supplierId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Supplier with Id {supplierId} not found");

        if (!BCrypt.Net.BCrypt.Verify(password, supplier.PasswordHash))
            throw new InvalidOperationException("Invalid password.");

        _supplierMapper.AnonymizeSupplierEntity(supplier);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Anonymized supplier with Id {SupplierId}", supplierId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
    {
        var suppliers = await _dbContext.Suppliers
            .Include(s => s.ZipCodeNavigation)
            .ToListAsync();
        return suppliers.Select(s => _supplierMapper.ToSupplierDto(s));
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<SupplierDto?> GetSupplierByIdAsync(Guid supplierId)
    {
        var supplier = await _dbContext.Suppliers
            .Include(s => s.ZipCodeNavigation)
            .Where(s => s.Id == supplierId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Supplier with Id {supplierId} not found");

        return supplier is null ? null : _supplierMapper.ToSupplierDto(supplier);
    }

    /// <inheritdoc />
    public Task<bool> IsEmailInUseAsync(string email, Guid? excludedSupplierId = null)
    {
        return _dbContext.Suppliers
            .AnyAsync(s => s.Email == email && (excludedSupplierId == null || s.Id != excludedSupplierId));
    }

    /// <inheritdoc />
    public Task<bool> IsPhoneNumberInUseAsync(string phoneNumber, Guid? excludedSupplierId = null)
    {
        return _dbContext.Suppliers
            .AnyAsync(s => s.PhoneNumber == phoneNumber && (excludedSupplierId == null || s.Id != excludedSupplierId));
    }

    /// <inheritdoc />
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<SupplierDto?> LoginSupplierAsync(SupplierLoginDto supplier)
    {
        var existingSupplier = await _dbContext.Suppliers
            .Include(s => s.ZipCodeNavigation)
            .Where(s => s.Email == supplier.Email)
            .FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(supplier.Password, existingSupplier.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return _supplierMapper.ToSupplierDto(existingSupplier);
    }

    /// <inheritdoc />
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task UpdateSupplierAsync(Guid supplierId, SupplierUpdateDto supplier)
    {
        var existingSupplier = await _dbContext.Suppliers
            .Where(s => s.Id == supplierId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Supplier with Id {supplierId} not found");

        if (supplier.Email is not null && await IsEmailInUseAsync(supplier.Email, supplierId))
            throw new InvalidOperationException("Email is already in use.");

        if (supplier.PhoneNumber is not null && await IsPhoneNumberInUseAsync(supplier.PhoneNumber, supplierId))
            throw new InvalidOperationException("Phone number is already in use.");

        _supplierMapper.UpdateSupplierEntity(existingSupplier, supplier);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated supplier with Id {SupplierId}", supplierId);
    }
}