using GroceryStore.Database.Entities;
using GroceryStore.Features.Customers.Interfaces;

namespace GroceryStore.Features.Customers;

/// <summary>
/// Mapper class for converting between Customer entities and DTOs.
/// </summary>
public class CustomerMapper : ICustomerMapper
{
    /// <inheritdoc/>
    public void AnonymizeCustomerEntity(Customer customerEntity)
    {
        customerEntity.Role = "anonymized_customer";
        customerEntity.LastName = "null";
        customerEntity.FirstName = "null";
        customerEntity.Email = $"anonymized_{Guid.NewGuid()}@system.local";
        customerEntity.PhoneNumber = null;
        customerEntity.Street = "null";
        customerEntity.HouseNumber = "null";
        customerEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
        customerEntity.UpdatedAtDateTime = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public CustomerDto ToCustomerDto(Customer customerEntity)
    {
        return new CustomerDto(
            CustomerId: customerEntity.Id,
            Role: customerEntity.Role,
            Name: customerEntity.FirstName + " " + customerEntity.LastName,
            BirthDate: customerEntity.BirthDate,
            Email: customerEntity.Email,
            PhoneNumber: customerEntity.PhoneNumber,
            Address: customerEntity.Street + " " + customerEntity.HouseNumber,
            Location: customerEntity.ZipCodeNavigation is null
                ? customerEntity.ZipCode
                : $"{customerEntity.ZipCodeNavigation.City}, {customerEntity.ZipCodeNavigation.ZipCode}",
            CreatedAtDateTime: customerEntity.CreatedAtDateTime,
            UpdatedAtDateTime: customerEntity.UpdatedAtDateTime
        );
    }

    /// <inheritdoc/>
    public Customer ToCustomerEntity(CustomerRegistrationDto customerRegistrationDto)
    {
        return new Customer
        {
            FirstName = customerRegistrationDto.FirstName,
            LastName = customerRegistrationDto.LastName,
            Email = customerRegistrationDto.Email,
            BirthDate = customerRegistrationDto.BirthDate,
            PhoneNumber = customerRegistrationDto.PhoneNumber,
            Street = customerRegistrationDto.Street,
            HouseNumber = customerRegistrationDto.HouseNumber,
            ZipCode = customerRegistrationDto.ZipCode,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(customerRegistrationDto.Password)
        };
    }

    /// <inheritdoc/>
    public void UpdateCustomerEntity(Customer customerEntity, CustomerUpdateDto customerUpdateDto)
    {
        if (customerUpdateDto.FirstName != null)
        {
            customerEntity.FirstName = customerUpdateDto.FirstName;
        }

        if (customerUpdateDto.LastName != null)
        {
            customerEntity.LastName = customerUpdateDto.LastName;
        }

        if (customerUpdateDto.Email != null)
        {
            customerEntity.Email = customerUpdateDto.Email;
        }

        if (customerUpdateDto.BirthDate != null)
        {
            customerEntity.BirthDate = customerUpdateDto.BirthDate.Value;
        }

        if (customerUpdateDto.PhoneNumber != null)
        {
            customerEntity.PhoneNumber = customerUpdateDto.PhoneNumber;
        }

        if (customerUpdateDto.Street != null)
        {
            customerEntity.Street = customerUpdateDto.Street;
        }

        if (customerUpdateDto.HouseNumber != null)
        {
            customerEntity.HouseNumber = customerUpdateDto.HouseNumber;
        }

        if (customerUpdateDto.ZipCode != null)
        {
            customerEntity.ZipCode = customerUpdateDto.ZipCode;
        }

        if (customerUpdateDto.Password != null && customerUpdateDto.ConfirmPassword != null)
        {
            if (customerUpdateDto.Password == customerUpdateDto.ConfirmPassword)
            {
                customerEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(customerUpdateDto.Password);
            }
            else
            {
                throw new ArgumentException("Password and Confirm Password do not match.");
            }
        }

        customerEntity.UpdatedAtDateTime = DateTime.UtcNow;
    }
}