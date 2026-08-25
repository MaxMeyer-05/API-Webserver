namespace GroceryStore.Features.Suppliers.Interfaces;

/// <summary>
/// Interface for supplier service operations.
/// </summary>
public interface ISupplierService
{
    /// <summary>
    /// Retrieves all suppliers from the database.
    /// </summary>
    /// <returns>A collection of SupplierDto objects.</returns>
    Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();

    /// <summary>
    /// Retrieves a supplier by its unique identifier.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <returns>A SupplierDto object if found; otherwise, null.</returns>
    Task<SupplierDto?> GetSupplierByIdAsync(Guid supplierId);

    /// <summary>
    /// Checks if an email is already in use by another supplier.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="excludedSupplierId">An optional supplier ID to exclude from the check.</param>
    /// <returns>True if the email is in use; otherwise, false.</returns>
    Task<bool> IsEmailInUseAsync(string email, Guid? excludedSupplierId = null);

    /// <summary>
    /// Checks if a phone number is already in use by another supplier.
    /// </summary>
    /// <param name="phoneNumber">The phone number to check.</param>
    /// <param name="excludedSupplierId">An optional supplier ID to exclude from the check.</param>
    /// <returns>True if the phone number is in use; otherwise, false.</returns>
    Task<bool> IsPhoneNumberInUseAsync(string phoneNumber, Guid? excludedSupplierId = null);

    /// <summary>
    /// Creates a new supplier in the database.
    /// </summary>
    /// <param name="supplier">The supplier registration data transfer object.</param>
    /// <returns>The created SupplierDto object.</returns>
    Task<SupplierDto> CreateSupplierAsync(SupplierRegistrationDto supplier);

    /// <summary>
    /// Updates an existing supplier in the database.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier to update.</param>
    /// <param name="supplier">The supplier update data transfer object.</param>
    Task UpdateSupplierAsync(Guid supplierId, SupplierUpdateDto supplier);

    /// <summary>
    /// Deletes a supplier from the database.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier to delete.</param>
    /// <param name="password">The password of the supplier for verification.</param>
    Task DeleteSupplierAsync(Guid supplierId, string password);

    /// <summary>
    /// Logs in a supplier using the provided credentials.
    /// </summary>
    /// <param name="supplier">The supplier login data transfer object.</param>
    /// <returns>A SupplierDto object if login is successful; otherwise, null.</returns>
    Task<SupplierDto?> LoginSupplierAsync(SupplierLoginDto supplier);
}