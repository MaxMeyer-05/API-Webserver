using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Security;
using SharedKernel.Tests.TestData;

namespace SharedKernel.Tests.Security;

[Trait("Category", "Security")]
[Trait("Module", "SharedKernel")]
public class JwtTokenServiceTest
{
    #region Token Generation Tests

    [Fact]
    [Trait("Feature", "TokenGeneration")]
    public void GenerateToken_ShouldReturnNonEmptyThreePartJwt()
    {
        // Arrange
        var options = SecurityTestData.CreateOptions();
        var service = new JwtTokenService(options);
        var userId = Guid.NewGuid();

        // Act
        var token = service.GenerateToken(userId, "john.doe@example.com", "user");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        var segments = token.Split('.');
        Assert.Equal(3, segments.Length);
    }

    [Fact]
    [Trait("Feature", "TokenGeneration")]
    public void GenerateToken_ShouldEmbedCoreAndCustomClaims()
    {
        // Arrange
        var settings = SecurityTestData.CreateJwtSettings();
        var options = SecurityTestData.CreateOptions(settings);
        var service = new JwtTokenService(options);

        var userId = Guid.NewGuid();
        const string email = "admin@example.com";
        const string role = "admin";
        var customClaims = new Dictionary<string, string>
        {
            ["tenant_id"] = "tenant-001",
            ["department"] = "IT"
        };

        // Act
        var token = service.GenerateToken(userId, email, role, customClaims);

        // Assert
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);

        Assert.Equal(settings.Issuer, jwt.Issuer);
        Assert.Contains(settings.Audience, jwt.Audiences);
        Assert.Equal(userId.ToString(), jwt.Subject);

        var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal(email, emailClaim.Value);

        var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role").Select(c => c.Value);
        Assert.Contains(role, roleClaims);

        var jtiClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.NotNull(jtiClaim);
        Assert.True(Guid.TryParse(jtiClaim.Value, out _));

        var tenantClaim = jwt.Claims.FirstOrDefault(c => c.Type == "tenant_id");
        Assert.NotNull(tenantClaim);
        Assert.Equal("tenant-001", tenantClaim.Value);

        var deptClaim = jwt.Claims.FirstOrDefault(c => c.Type == "department");
        Assert.NotNull(deptClaim);
        Assert.Equal("IT", deptClaim.Value);
    }

    [Fact]
    [Trait("Feature", "TokenGeneration")]
    public void GenerateToken_ShouldSetAccurateExpirationTime()
    {
        // Arrange
        const int expirationMinutes = 45;
        var settings = SecurityTestData.CreateJwtSettings(expirationMinutes: expirationMinutes);
        var service = new JwtTokenService(SecurityTestData.CreateOptions(settings));

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = service.GenerateToken(Guid.NewGuid(), "test@domain.com", "user");

        // Assert
        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);

        var expectedExpiration = beforeGeneration.AddMinutes(expirationMinutes);
        var actualExpiration = jwt.ValidTo;

        Assert.InRange(actualExpiration, expectedExpiration.AddSeconds(-5), expectedExpiration.AddSeconds(5));
    }

    #endregion

    #region Signature Verification Tests

    [Fact]
    [Trait("Feature", "TokenValidation")]
    public async Task GenerateToken_ShouldBeValidatableWithConfiguredSymmetricKey()
    {
        // Arrange
        var settings = SecurityTestData.CreateJwtSettings();
        var service = new JwtTokenService(SecurityTestData.CreateOptions(settings));
        var userId = Guid.NewGuid();

        var token = service.GenerateToken(userId, "verified@example.com", "manager");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JsonWebTokenHandler();

        // Act
        var validationResult = await handler.ValidateTokenAsync(token, validationParameters);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.NotNull(validationResult.ClaimsIdentity);
        Assert.Equal(userId.ToString(), validationResult.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
    }

    [Fact]
    [Trait("Feature", "TokenValidation")]
    public async Task GenerateToken_ShouldFailValidation_WhenValidatedWithDifferentKey()
    {
        // Arrange
        var service = new JwtTokenService(SecurityTestData.CreateOptions());
        var token = service.GenerateToken(Guid.NewGuid(), "invalid-sig@example.com", "user");

        const string wrongKey = "completely_different_signing_key_that_is_32_bytes_long!";
        var invalidValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(wrongKey)),
            ValidateLifetime = false
        };

        var handler = new JsonWebTokenHandler();

        // Act
        var validationResult = await handler.ValidateTokenAsync(token, invalidValidationParameters);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.NotNull(validationResult.Exception);
    }

    #endregion
}