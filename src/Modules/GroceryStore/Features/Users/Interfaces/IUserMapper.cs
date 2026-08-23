using GroceryStore.Database.Entities;

namespace GroceryStore.Features.Users.Interfaces;
public interface IUserMapper
{
    /// <summary>
    /// Maps a <see cref="UserRegistrationDto"/> to a <see cref="User"/> entity.
    /// </summary>
    /// <param name="userRegistrationDto">The <see cref="UserRegistrationDto"/> to map.</param>
    /// <returns>The mapped <see cref="User"/> entity.</returns>
    User ToUserEntity(UserRegistrationDto userRegistrationDto);

    /// <summary>
    /// Maps a <see cref="User"/> entity to a <see cref="UserDto"/>.
    /// </summary>
    /// <param name="userEntity">The <see cref="User"/> entity to map.</param>
    /// <returns>The mapped <see cref="UserDto"/>.</returns>
    UserDto ToUserDto(User userEntity);

    /// <summary>
    /// Updates an existing <see cref="User"/> entity with values from a <see cref="UserUpdateDto"/>.
    /// </summary>
    /// <param name="userEntity">The <see cref="User"/> entity to update.</param>
    /// <param name="userUpdateDto">The <see cref="UserUpdateDto"/> containing updated values.</param>
    void UpdateUserEntity(User userEntity, UserUpdateDto userUpdateDto);
    
    /// <summary>
    /// Anonymizes an existing <see cref="User"/> entity by removing sensitive information.
    /// </summary>
    /// <param name="userEntity">The <see cref="User"/> entity to anonymize.</param>
    void AnonymizeUserEntity(User userEntity);
}