namespace SharedKernel.Security;

/// <summary>
/// Represents the settings for JWT (JSON Web Token) authentication.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// The name of the configuration section for JWT settings.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Gets or sets the secret key used for signing the JWT.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the issuer of the JWT.
    /// </summary>
    public string Issuer { get; set; } = "API-WebServer";

    /// <summary>
    /// Gets or sets the audience for the JWT.
    /// </summary>
    public string Audience { get; set; } = "API-WebServer-Clients";

    /// <summary>
    /// Gets or sets the expiration time of the JWT in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
}