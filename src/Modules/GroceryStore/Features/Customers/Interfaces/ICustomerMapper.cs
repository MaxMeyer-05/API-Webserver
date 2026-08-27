using GroceryStore.Database.Entities;

namespace GroceryStore.Features.Customers.Interfaces;
public interface ICustomerMapper
{
    /// <summary>
    /// Maps a <see cref="CustomerRegistrationDto"/> to a <see cref="Customer"/> entity.
    /// </summary>
    /// <param name="customerRegistrationDto">The <see cref="CustomerRegistrationDto"/> to map.</param>
    /// <returns>The mapped <see cref="Customer"/> entity.</returns>
    Customer ToCustomerEntity(CustomerRegistrationDto customerRegistrationDto);

    /// <summary>
    /// Maps a <see cref="Customer"/> entity to a <see cref="CustomerDto"/>.
    /// </summary>
    /// <param name="customerEntity">The <see cref="Customer"/> entity to map.</param>
    /// <returns>The mapped <see cref="CustomerDto"/>.</returns>
    CustomerDto ToCustomerDto(Customer customerEntity);

    /// <summary>
    /// Updates an existing <see cref="Customer"/> entity with values from a <see cref="CustomerUpdateDto"/>.
    /// </summary>
    /// <param name="customerEntity">The <see cref="Customer"/> entity to update.</param>
    /// <param name="customerUpdateDto">The <see cref="CustomerUpdateDto"/> containing updated values.</param>
    void UpdateCustomerEntity(Customer customerEntity, CustomerUpdateDto customerUpdateDto);
    
    /// <summary>
    /// Anonymizes an existing <see cref="Customer"/> entity by removing sensitive information.
    /// </summary>
    /// <param name="customerEntity">The <see cref="Customer"/> entity to anonymize.</param>
    void AnonymizeCustomerEntity(Customer customerEntity);
}