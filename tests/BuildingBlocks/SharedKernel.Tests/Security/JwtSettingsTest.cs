using SharedKernel.Security;

namespace SharedKernel.Tests.Security;

[Trait("Category", "Security")]
[Trait("Module", "SharedKernel")]
public class JwtSettingsTest
{
    #region Default Values Tests

    [Fact]
    [Trait("Feature", "Configuration")]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var settings = new JwtSettings();

        // Assert
        Assert.Equal("Jwt", JwtSettings.SectionName);
        Assert.Equal(string.Empty, settings.SecretKey);
        Assert.Equal("API-WebServer", settings.Issuer);
        Assert.Equal("API-WebServer-Clients", settings.Audience);
        Assert.Equal(60, settings.ExpirationMinutes);
    }

    #endregion
}