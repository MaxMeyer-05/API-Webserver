using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using SharedKernel.Security;
using SharedKernel.Security.Interfaces;
using SharedKernel.Tests.TestData;

namespace SharedKernel.Tests.Security;

[Trait("Category", "Security")]
[Trait("Module", "SharedKernel")]
public class CurrentUserTest
{
    #region Authentication Status Tests

    [Fact]
    [Trait("Feature", "Authentication")]
    public void IsAuthenticated_ShouldReturnTrue_WhenUserIsAuthenticated()
    {
        // Arrange
        var principal = SecurityTestData.CreateClaimsPrincipal(isAuthenticated: true);
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.IsAuthenticated;

        // Assert
        Assert.True(result);
    }

    [Fact]
    [Trait("Feature", "Authentication")]
    public void IsAuthenticated_ShouldReturnFalse_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var principal = SecurityTestData.CreateClaimsPrincipal(isAuthenticated: false);
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    [Fact]
    [Trait("Feature", "Authentication")]
    public void IsAuthenticated_ShouldReturnFalse_WhenHttpContextIsNull()
    {
        // Arrange
        var accessor = SecurityTestData.CreateHttpContextAccessor(hasHttpContext: false);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    #endregion

    #region UserId Resolution Tests

    [Fact]
    [Trait("Feature", "Claims")]
    public void UserId_ShouldReturnGuid_WhenJwtSubClaimIsPresent()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var principal = SecurityTestData.CreateClaimsPrincipal(userId: expectedUserId, useJwtClaimNames: true);
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.UserId;

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    [Fact]
    [Trait("Feature", "Claims")]
    public void UserId_ShouldReturnGuid_WhenNameIdentifierClaimIsPresent()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var principal = SecurityTestData.CreateClaimsPrincipal(userId: expectedUserId, useJwtClaimNames: false);
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.UserId;

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    [Fact]
    [Trait("Feature", "Claims")]
    public void UserId_ShouldThrowInvalidOperationException_WhenClaimIsMissing()
    {
        // Arrange
        var principal = SecurityTestData.CreateClaimsPrincipal(userId: null);
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => currentUser.UserId);
        Assert.Equal("User ID is not available.", exception.Message);
    }

    [Fact]
    [Trait("Feature", "Claims")]
    public void UserId_ShouldThrowInvalidOperationException_WhenClaimIsNotAValidGuid()
    {
        // Arrange
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, "invalid-guid-string") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => currentUser.UserId);
        Assert.Equal("User ID is not available.", exception.Message);
    }

    [Fact]
    [Trait("Feature", "Claims")]
    public void UserId_ShouldThrowInvalidOperationException_WhenHttpContextIsNull()
    {
        // Arrange
        var accessor = SecurityTestData.CreateHttpContextAccessor(hasHttpContext: false);
        var currentUser = new CurrentUser(accessor);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => currentUser.UserId);
        Assert.Equal("User ID is not available.", exception.Message);
    }

    #endregion

    #region Role & Email Resolution Tests

    [Theory]
    [InlineData("admin", true)]
    [InlineData("manager", false)]
    [Trait("Feature", "Claims")]
    public void Role_ShouldResolveFromBothStandardAndCustomRoleClaims(string roleName, bool useJwtClaimName)
    {
        // Arrange
        var principal = SecurityTestData.CreateClaimsPrincipal(role: roleName, useJwtClaimNames: useJwtClaimName);
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.Role;

        // Assert
        Assert.Equal(roleName, result);
    }

    [Fact]
    [Trait("Feature", "Claims")]
    public void Role_ShouldReturnNull_WhenRoleClaimIsMissing()
    {
        // Arrange
        var principal = SecurityTestData.CreateClaimsPrincipal(role: null);
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.Role;

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("employee@corp.local", false)]
    [Trait("Feature", "Claims")]
    public void Email_ShouldResolveFromBothJwtAndClaimTypesEmail(string emailAddress, bool useJwtClaimName)
    {
        // Arrange
        var principal = SecurityTestData.CreateClaimsPrincipal(email: emailAddress, useJwtClaimNames: useJwtClaimName);
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.Email;

        // Assert
        Assert.Equal(emailAddress, result);
    }

    [Fact]
    [Trait("Feature", "Claims")]
    public void Email_ShouldReturnNull_WhenEmailClaimIsMissing()
    {
        // Arrange
        var principal = SecurityTestData.CreateClaimsPrincipal(email: null);
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.Email;

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region IsInRole Tests

    [Fact]
    [Trait("Feature", "Authorization")]
    public void IsInRole_ShouldReturnTrue_WhenUserHasRole()
    {
        // Arrange
        var claims = new List<Claim> { new(ClaimTypes.Role, "admin") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.IsInRole("admin");

        // Assert
        Assert.True(result);
    }

    [Fact]
    [Trait("Feature", "Authorization")]
    public void IsInRole_ShouldReturnFalse_WhenUserDoesNotHaveRole()
    {
        // Arrange
        var claims = new List<Claim> { new(ClaimTypes.Role, "user") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var accessor = SecurityTestData.CreateHttpContextAccessor(principal);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.IsInRole("admin");

        // Assert
        Assert.False(result);
    }

    [Fact]
    [Trait("Feature", "Authorization")]
    public void IsInRole_ShouldReturnFalse_WhenHttpContextIsNull()
    {
        // Arrange
        var accessor = SecurityTestData.CreateHttpContextAccessor(hasHttpContext: false);
        var currentUser = new CurrentUser(accessor);

        // Act
        var result = currentUser.IsInRole("admin");

        // Assert
        Assert.False(result);
    }

    #endregion
}