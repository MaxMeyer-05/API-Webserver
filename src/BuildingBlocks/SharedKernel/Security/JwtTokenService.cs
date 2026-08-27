using System.Text;
using System.Security.Claims;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using SharedKernel.Security.Interfaces;

namespace SharedKernel.Security;
/// <summary>
/// Represents a service for generating JWT tokens.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    /// <inheritdoc />
    public string GenerateToken(Guid userId, string email, string role, IDictionary<string, string>? customClaims = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role),
            new("role", role)
        };

        if (customClaims != null)
        {
            foreach (var (key, value) in customClaims)
            {
                claims.Add(new Claim(key, value));
            }
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            SigningCredentials = credentials
        };

        return _tokenHandler.CreateToken(tokenDescriptor);
    }
}