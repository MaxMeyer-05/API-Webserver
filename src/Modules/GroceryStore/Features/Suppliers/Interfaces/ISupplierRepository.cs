namespace GroceryStore.Features.Suppliers.Interfaces;
public interface ISupplierRepository
{
    Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();
    Task<SupplierDto?> GetSupplierByIdAsync(Guid supplierId);
    Task<SupplierDto> CreateSupplierAsync(SupplierRegistrationDto supplier);
    Task<SupplierDto?> LoginSupplierAsync(SupplierLoginDto supplier);
    Task UpdateSupplierAsync(Guid supplierId, SupplierUpdateDto supplier);
    Task DeleteSupplierAsync(Guid supplierId);
}