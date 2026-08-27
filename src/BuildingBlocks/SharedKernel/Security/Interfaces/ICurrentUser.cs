namespace SharedKernel.Security.Interfaces;

/// <summary>
/// Represents the current user in the application.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Gets the role of the current user.
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Gets the email of the current user.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if the current user is in the specified role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the current user is in the specified role; otherwise, false.</returns>
    bool IsInRole(string role);
}