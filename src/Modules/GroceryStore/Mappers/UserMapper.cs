using GroceryStore.Models;
using GroceryStore.Database.Entities;
using GroceryStore.Mappers.Interfaces;

namespace GroceryStore.Mappers;

/// <summary>
/// Mapper class for converting between User entities and DTOs.
/// </summary>
public class UserMapper : IUserMapper
{
    /// <inheritdoc/>
    public void AnonymizeUserEntity(User userEntity)
    {
        userEntity.Role = "anonymized_user";
        userEntity.LastName = "null";
        userEntity.FirstName = "null";
        userEntity.Email = $"anonymized_{Guid.NewGuid()}@system.local";
        userEntity.PhoneNumber = null;
        userEntity.Street = "null";
        userEntity.HouseNumber = "null";
        userEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
        userEntity.UpdatedAtDateTime = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public UserDto ToUserDto(User userEntity)
    {
        return new UserDto(
            Role: userEntity.Role,
            Name: userEntity.FirstName + " " + userEntity.LastName,
            BirthDate: userEntity.BirthDate,
            Email: userEntity.Email,
            PhoneNumber: userEntity.PhoneNumber,
            Address: userEntity.Street + " " + userEntity.HouseNumber,
            Location: userEntity.ZipCodeNavigation?.City + ", " + userEntity.ZipCodeNavigation?.ZipCode,
            CreatedAtDateTime: userEntity.CreatedAtDateTime,
            UpdatedAtDateTime: userEntity.UpdatedAtDateTime
        );
    }

    /// <inheritdoc/>
    public User ToUserEntity(UserRegistrationDto userRegistrationDto)
    {
        return new User
        {
            FirstName = userRegistrationDto.FirstName,
            LastName = userRegistrationDto.LastName,
            Email = userRegistrationDto.Email,
            BirthDate = userRegistrationDto.BirthDate,
            PhoneNumber = userRegistrationDto.PhoneNumber,
            Street = userRegistrationDto.Street,
            HouseNumber = userRegistrationDto.HouseNumber,
            ZipCode = userRegistrationDto.ZipCode,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegistrationDto.Password)
        };
    }

    /// <inheritdoc/>
    public void UpdateUserEntity(User userEntity, UserUpdateDto userUpdateDto)
    {
        if (userUpdateDto.FirstName != null)
        {
            userEntity.FirstName = userUpdateDto.FirstName;
        }

        if (userUpdateDto.LastName != null)
        {
            userEntity.LastName = userUpdateDto.LastName;
        }

        if (userUpdateDto.Email != null)
        {
            userEntity.Email = userUpdateDto.Email;
        }

        if (userUpdateDto.BirthDate != null)
        {
            userEntity.BirthDate = userUpdateDto.BirthDate.Value;
        }

        if (userUpdateDto.PhoneNumber != null)
        {
            userEntity.PhoneNumber = userUpdateDto.PhoneNumber;
        }

        if (userUpdateDto.Street != null)
        {
            userEntity.Street = userUpdateDto.Street;
        }

        if (userUpdateDto.HouseNumber != null)
        {
            userEntity.HouseNumber = userUpdateDto.HouseNumber;
        }

        if (userUpdateDto.ZipCode != null)
        {
            userEntity.ZipCode = userUpdateDto.ZipCode;
        }

        if (userUpdateDto.Password != null && userUpdateDto.ConfirmPassword != null)
        {
            if (userUpdateDto.Password == userUpdateDto.ConfirmPassword)
            {
                userEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userUpdateDto.Password);
            }
            else
            {
                throw new ArgumentException("Password and Confirm Password do not match.");
            }
        }

        userEntity.UpdatedAtDateTime = DateTime.UtcNow;
    }
}