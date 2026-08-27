namespace GroceryStore.Features.Customers.Interfaces;

/// <summary>
/// Interface for customer repository operations.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Retrieves all customers from the database.
    /// </summary>
    /// <returns>A collection of CustomerDto objects.</returns>
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();

    /// <summary>
    /// Retrieves a customer by its unique identifier.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>A CustomerDto object if found; otherwise, null.</returns>
    Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId);

    /// <summary>
    /// Checks if an email is already in use by another customer.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="excludedCustomerId">An optional customer ID to exclude from the check.</param>
    /// <returns>True if the email is in use; otherwise, false.</returns>
    Task<bool> IsEmailInUseAsync(string email, Guid? excludedCustomerId = null);

    /// <summary>
    /// Checks if a phone number is already in use by another customer.
    /// </summary>
    /// <param name="phoneNumber">The phone number to check.</param>
    /// <param name="excludedCustomerId">An optional customer ID to exclude from the check.</param>
    /// <returns>True if the phone number is in use; otherwise, false.</returns>
    Task<bool> IsPhoneNumberInUseAsync(string phoneNumber, Guid? excludedCustomerId = null);

    /// <summary>
    /// Creates a new customer in the database.
    /// </summary>
    /// <param name="customer">The customer registration data transfer object.</param>
    /// <returns>The created CustomerDto object.</returns>
    Task<CustomerDto> CreateCustomerAsync(CustomerRegistrationDto customer);

    /// <summary>
    /// Updates an existing customer in the database.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer to update.</param>
    /// <param name="customer">The customer update data transfer object.</param>
    Task UpdateCustomerAsync(Guid customerId, CustomerUpdateDto customer);

    /// <summary>
    /// Deletes a customer from the database.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer to delete.</param>
    /// <param name="password">The password of the customer to confirm deletion.</param>
    Task DeleteCustomerAsync(Guid customerId, string password);

    /// <summary>
    /// Logs in a customer using the provided credentials.
    /// </summary>
    /// <param name="customer">The customer login data transfer object.</param>
    /// <returns>A CustomerDto object if login is successful; otherwise, null.</returns>
    Task<CustomerDto?> LoginCustomerAsync(CustomerLoginDto customer);
}