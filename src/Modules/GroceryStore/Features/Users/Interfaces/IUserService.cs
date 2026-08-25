namespace GroceryStore.Features.Users.Interfaces;

/// <summary>
/// Interface for user repository operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves all users from the database.
    /// </summary>
    /// <returns>A collection of UserDto objects.</returns>
    Task<IEnumerable<UserDto>> GetAllUsersAsync();

    /// <summary>
    /// Retrieves a user by its unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A UserDto object if found; otherwise, null.</returns>
    Task<UserDto?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Checks if an email is already in use by another user.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="excludedUserId">An optional user ID to exclude from the check.</param>
    /// <returns>True if the email is in use; otherwise, false.</returns>
    Task<bool> IsEmailInUseAsync(string email, Guid? excludedUserId = null);

    /// <summary>
    /// Checks if a phone number is already in use by another user.
    /// </summary>
    /// <param name="phoneNumber">The phone number to check.</param>
    /// <param name="excludedUserId">An optional user ID to exclude from the check.</param>
    /// <returns>True if the phone number is in use; otherwise, false.</returns>
    Task<bool> IsPhoneNumberInUseAsync(string phoneNumber, Guid? excludedUserId = null);

    /// <summary>
    /// Creates a new user in the database.
    /// </summary>
    /// <param name="user">The user registration data transfer object.</param>
    /// <returns>The created UserDto object.</returns>
    Task<UserDto> CreateUserAsync(UserRegistrationDto user);

    /// <summary>
    /// Updates an existing user in the database.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="user">The user update data transfer object.</param>
    Task UpdateUserAsync(Guid userId, UserUpdateDto user);

    /// <summary>
    /// Deletes a user from the database.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <param name="password">The password of the user to confirm deletion.</param>
    Task DeleteUserAsync(Guid userId, string password);

    /// <summary>
    /// Logs in a user using the provided credentials.
    /// </summary>
    /// <param name="user">The user login data transfer object.</param>
    /// <returns>A UserDto object if login is successful; otherwise, null.</returns>
    Task<UserDto?> LoginUserAsync(UserLoginDto user);
}