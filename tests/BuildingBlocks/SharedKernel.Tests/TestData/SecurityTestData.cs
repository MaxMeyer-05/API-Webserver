using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using SharedKernel.Security;

namespace SharedKernel.Tests.TestData;

/// <summary>
/// Provides test data and helper methods for security-related tests, 
/// including JWT settings, HTTP context, and claims principal creation.
/// </summary>
public static class SecurityTestData
{
    #region Settings & Options Fixtures

    public const string DefaultSecretKey = "super_secret_test_key_with_sufficient_length_32_bytes_long!";
    public const string DefaultIssuer = "API-WebServer";
    public const string DefaultAudience = "API-WebServer-Clients";
    public const int DefaultExpirationMinutes = 60;

    /// <summary>
    /// Creates a new instance of JwtSettings with default or specified values for testing purposes.
    /// </summary>
    /// <param name="secretKey">The secret key used for signing the JWT.</param>
    /// <param name="issuer">The issuer of the JWT.</param>
    /// <param name="audience">The audience for the JWT.</param>
    /// <param name="expirationMinutes">The expiration time of the JWT in minutes.</param>
    /// <returns>A new instance of <see cref="JwtSettings"/> with the specified values.</returns>
    public static JwtSettings CreateJwtSettings(
        string secretKey = DefaultSecretKey,
        string issuer = DefaultIssuer,
        string audience = DefaultAudience,
        int expirationMinutes = DefaultExpirationMinutes) => new()
    {
        SecretKey = secretKey,
        Issuer = issuer,
        Audience = audience,
        ExpirationMinutes = expirationMinutes
    };

    /// <summary>
    /// Creates an IOptions<JwtSettings> instance for testing purposes, 
    /// wrapping the provided JwtSettings or creating a new one with default values.
    /// </summary>
    /// <param name="settings">The JwtSettings instance to wrap in IOptions.</param>
    /// <returns>An IOptions<JwtSettings> instance containing the provided or default JwtSettings.</returns>
    public static IOptions<JwtSettings> CreateOptions(JwtSettings? settings = null)
    {
        return Options.Create(settings ?? CreateJwtSettings());
    }

    #endregion

    #region HttpContext & Claims Fixtures

    /// <summary>
    /// Creates a mock IHttpContextAccessor for testing purposes.
    /// </summary>
    /// <param name="principal">The ClaimsPrincipal to set in the HTTP context. If null, no user will be set.</param>
    /// <param name="hasHttpContext">Indicates whether to create an HTTP context or return null. If false, the HttpContext will be null.</param>
    /// <returns>A mock IHttpContextAccessor with the specified ClaimsPrincipal and HTTP context.</returns>
    public static IHttpContextAccessor CreateHttpContextAccessor(
        ClaimsPrincipal? principal = null,
        bool hasHttpContext = true)
    {
        var accessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);

        if (!hasHttpContext)
        {
            accessorMock.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);
            return accessorMock.Object;
        }

        var context = new DefaultHttpContext();
        if (principal is not null)
        {
            context.User = principal;
        }

        accessorMock.SetupGet(a => a.HttpContext).Returns(context);
        return accessorMock.Object;
    }

    /// <summary>
    /// Creates a ClaimsPrincipal with specified claims for testing purposes.
    /// </summary>
    /// <param name="userId">The user ID to include in the claims. If null, no user ID claim will be added.</param>
    /// <param name="email">The email to include in the claims. If null, no email claim will be added.</param>
    /// <param name="role">The role to include in the claims. If null, no role claim will be added.</param>
    /// <param name="isAuthenticated">Indicates whether the ClaimsPrincipal should be authenticated.</param>
    /// <param name="useJwtClaimNames">
    /// Indicates whether to use JWT claim names (e.g., "sub", "email") 
    /// or standard claim types (e.g., ClaimTypes.NameIdentifier, ClaimTypes.Email).
    /// </param>
    /// <param name="additionalClaims">A dictionary of additional claims to include in the ClaimsPrincipal.</param>
    /// <returns>A ClaimsPrincipal with the specified claims and authentication status.</returns>
    public static ClaimsPrincipal CreateClaimsPrincipal(
        Guid? userId = null,
        string? email = null,
        string? role = null,
        bool isAuthenticated = true,
        bool useJwtClaimNames = true,
        IDictionary<string, string>? additionalClaims = null)
    {
        var claims = new List<Claim>();

        if (userId.HasValue)
        {
            var claimType = useJwtClaimNames ? JwtRegisteredClaimNames.Sub : ClaimTypes.NameIdentifier;
            claims.Add(new Claim(claimType, userId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var claimType = useJwtClaimNames ? JwtRegisteredClaimNames.Email : ClaimTypes.Email;
            claims.Add(new Claim(claimType, email));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var claimType = useJwtClaimNames ? "role" : ClaimTypes.Role;
            claims.Add(new Claim(claimType, role));
        }

        if (additionalClaims is not null)
        {
            foreach (var (key, value) in additionalClaims)
            {
                claims.Add(new Claim(key, value));
            }
        }

        var identity = isAuthenticated
            ? new ClaimsIdentity(claims, "TestAuthType")
            : new ClaimsIdentity(claims);

        return new ClaimsPrincipal(identity);
    }

    #endregion
}