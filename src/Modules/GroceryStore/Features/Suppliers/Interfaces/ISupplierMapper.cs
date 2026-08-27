using GroceryStore.Database.Entities;

namespace GroceryStore.Features.Suppliers.Interfaces;
public interface ISupplierMapper
{
    /// <summary>
    /// Maps a <see cref="SupplierRegistrationDto"/> to a <see cref="Supplier"/> entity.
    /// </summary>
    /// <param name="supplierRegistrationDto">The <see cref="SupplierRegistrationDto"/> to map.</param>
    /// <returns>The mapped <see cref="Supplier"/> entity.</returns>
    Supplier ToSupplierEntity(SupplierRegistrationDto supplierRegistrationDto);

    /// <summary>
    /// Maps a <see cref="Supplier"/> entity to a <see cref="SupplierDto"/>.
    /// </summary>
    /// <param name="supplierEntity">The <see cref="Supplier"/> entity to map.</param>
    /// <returns>The mapped <see cref="SupplierDto"/>.</returns>
    SupplierDto ToSupplierDto(Supplier supplierEntity);

    /// <summary>
    /// Updates an existing <see cref="Supplier"/> entity with values from a <see cref="SupplierUpdateDto"/>.
    /// </summary>
    /// <param name="supplierEntity">The <see cref="Supplier"/> entity to update.</param>
    /// <param name="supplierUpdateDto">The <see cref="SupplierUpdateDto"/> containing updated values.</param>
    void UpdateSupplierEntity(Supplier supplierEntity, SupplierUpdateDto supplierUpdateDto);

    /// <summary>
    /// Anonymizes an existing <see cref="Supplier"/> entity by removing sensitive information.
    /// </summary>
    /// <param name="supplierEntity">The <see cref="Supplier"/> entity to anonymize.</param>
    void AnonymizeSupplierEntity(Supplier supplierEntity);
}