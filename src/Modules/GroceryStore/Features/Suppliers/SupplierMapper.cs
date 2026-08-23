using GroceryStore.Database.Entities;
using GroceryStore.Features.Suppliers.Interfaces;

namespace GroceryStore.Features.Suppliers;

/// <summary>
/// Mapper class for converting between Supplier entities and DTOs.
/// </summary>
public class SupplierMapper : ISupplierMapper
{
    /// <inheritdoc/>
    public void AnonymizeSupplierEntity(Supplier supplierEntity)
    {
        supplierEntity.Role = "anonymized_supplier";
        supplierEntity.CompanyName = "Anonymized Supplier";
        supplierEntity.Street = "null";
        supplierEntity.HouseNumber = "null";
        supplierEntity.PhoneNumber = null;
        supplierEntity.Email = $"anonymized_{Guid.NewGuid()}@system.local";
        supplierEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
        supplierEntity.UpdatedAtDateTime = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public SupplierDto ToSupplierDto(Supplier supplierEntity)
    {
        return new SupplierDto(
            Role: supplierEntity.Role,
            CompanyName: supplierEntity.CompanyName,
            Address: supplierEntity.Street + " " + supplierEntity.HouseNumber,
            Location: supplierEntity.ZipCodeNavigation?.City + ", " + supplierEntity.ZipCodeNavigation?.ZipCode,
            PhoneNumber: supplierEntity.PhoneNumber,
            Email: supplierEntity.Email,
            CreatedAtDateTime: supplierEntity.CreatedAtDateTime,
            UpdatedAtDateTime: supplierEntity.UpdatedAtDateTime
        );
    }

    /// <inheritdoc/>
    public Supplier ToSupplierEntity(SupplierRegistrationDto supplierRegistrationDto)
    {
        return new Supplier
        {
            CompanyName = supplierRegistrationDto.CompanyName,
            Street = supplierRegistrationDto.Street,
            HouseNumber = supplierRegistrationDto.HouseNumber,
            ZipCode = supplierRegistrationDto.ZipCode,
            PhoneNumber = supplierRegistrationDto.PhoneNumber,
            Email = supplierRegistrationDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(supplierRegistrationDto.Password)
        };
    }

    /// <inheritdoc/>
    public void UpdateSupplierEntity(Supplier supplierEntity, SupplierUpdateDto supplierUpdateDto)
    {
        if (supplierUpdateDto.CompanyName != null)
        {
            supplierEntity.CompanyName = supplierUpdateDto.CompanyName;
        }

        if (supplierUpdateDto.Street != null)
        {
            supplierEntity.Street = supplierUpdateDto.Street;
        }

        if (supplierUpdateDto.HouseNumber != null)
        {
            supplierEntity.HouseNumber = supplierUpdateDto.HouseNumber;
        }

        if (supplierUpdateDto.ZipCode != null)
        {
            supplierEntity.ZipCode = supplierUpdateDto.ZipCode;
        }

        if (supplierUpdateDto.PhoneNumber != null)
        {
            supplierEntity.PhoneNumber = supplierUpdateDto.PhoneNumber;
        }

        if (supplierUpdateDto.Email != null)
        {
            supplierEntity.Email = supplierUpdateDto.Email;
        }

        if (supplierUpdateDto.Password != null && supplierUpdateDto.ConfirmPassword != null)
        {
            if (supplierUpdateDto.Password == supplierUpdateDto.ConfirmPassword)
            {
                supplierEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(supplierUpdateDto.Password);
            }
            else
            {
                throw new ArgumentException("Password and Confirm Password do not match.");
            }
        }

        supplierEntity.UpdatedAtDateTime = DateTime.UtcNow;
    }
}