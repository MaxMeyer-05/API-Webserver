using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

using SharedKernel.Security.Interfaces;

namespace SharedKernel.Security;

/// <summary>
/// Represents the current user in the application context.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current authenticated user as a ClaimsPrincipal.
    /// </summary>
    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <summary>
    /// Indicates whether the current user is authenticated.
    /// </summary>
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Gets the unique identifier of the current user, if available.
    /// </summary>
    public Guid UserId
    {
        get
        {
            var idString = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                        ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(idString, out var guid) ? guid 
                : throw new InvalidOperationException("User ID is not available.");
        }
    }

    /// <summary>
    /// Gets the username of the current user, if available.
    /// </summary>
    public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value 
                        ?? User?.FindFirst("role")?.Value;

    /// <summary>
    /// Gets the email address of the current user, if available.
    /// </summary>
    public string? Email => User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value 
                         ?? User?.FindFirst(ClaimTypes.Email)?.Value;

    /// <inheritdoc />
    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}