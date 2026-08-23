using GroceryStore.Features.Suppliers.Interfaces;

namespace GroceryStore.Features.Suppliers;

public class SupplierRepository : ISupplierRepository
{
    public Task<SupplierDto> CreateSupplierAsync(SupplierRegistrationDto supplier)
    {
        throw new NotImplementedException();
    }

    public Task DeleteSupplierAsync(Guid supplierId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<SupplierDto?> GetSupplierByIdAsync(Guid supplierId)
    {
        throw new NotImplementedException();
    }

    public Task<SupplierDto?> LoginSupplierAsync(SupplierLoginDto supplier)
    {
        throw new NotImplementedException();
    }

    public Task UpdateSupplierAsync(Guid supplierId, SupplierUpdateDto supplier)
    {
        throw new NotImplementedException();
    }
}