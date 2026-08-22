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
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public UserDto ToUserDto(User userEntity)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public User ToUserEntity(UserRegistrationDto userRegistrationDto)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void UpdateUserEntity(User userEntity, UserUpdateDto userUpdateDto)
    {
        throw new NotImplementedException();
    }
}