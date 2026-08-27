namespace SharedKernel.Security.Interfaces;

/// <summary>
/// Represents a service for generating tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a token for the specified user with the given claims.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="role">The role of the user.</param>
    /// <param name="customClaims">Additional custom claims to include in the token.</param>
    /// <returns>The generated token as a string.</returns>
    string GenerateToken(Guid userId, string email, string role, IDictionary<string, string>? customClaims = null);
}
