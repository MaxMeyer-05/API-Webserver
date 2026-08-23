using GroceryStore.Features.Suppliers.Interfaces;

namespace GroceryStore.Features.Suppliers;
public class SupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        ISupplierRepository supplierRepository,
        ILogger<SupplierService> logger)
    {
        _supplierRepository = supplierRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
    {
        var suppliers = await _supplierRepository.GetAllSuppliersAsync();
        return suppliers;
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(Guid supplierId)
    {
        var supplier = await _supplierRepository.GetSupplierByIdAsync(supplierId);
        return supplier;
    }

    public async Task<SupplierDto> CreateSupplierAsync(SupplierRegistrationDto supplierRegistrationDto)
    {
        var createdSupplier = await _supplierRepository.CreateSupplierAsync(supplierRegistrationDto);
        return createdSupplier;
    }

    public async Task UpdateSupplierAsync(Guid supplierId, SupplierUpdateDto supplierUpdateDto)
    {
        await _supplierRepository.UpdateSupplierAsync(supplierId, supplierUpdateDto);
    }

    public async Task DeleteSupplierAsync(Guid supplierId)
    {
        await _supplierRepository.DeleteSupplierAsync(supplierId);
    }
}