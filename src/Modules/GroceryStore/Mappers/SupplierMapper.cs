using GroceryStore.Models;
using GroceryStore.Database.Entities;
using GroceryStore.Mappers.Interfaces;

namespace GroceryStore.Mappers;

/// <summary>
/// Mapper class for converting between Supplier entities and DTOs.
/// </summary>
public class SupplierMapper : ISupplierMapper
{
    /// <inheritdoc/>
    public void AnonymizeSupplierEntity(Supplier supplierEntity)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public SupplierDto ToSupplierDto(Supplier supplierEntity)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Supplier ToSupplierEntity(SupplierRegistrationDto supplierRegistrationDto)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void UpdateSupplierEntity(Supplier supplierEntity, SupplierUpdateDto supplierUpdateDto)
    {
        throw new NotImplementedException();
    }
}