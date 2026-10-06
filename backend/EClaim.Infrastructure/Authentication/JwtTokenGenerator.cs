using System.IdentityModel.Tokens.Jwt;
using EClaim.Application.Interfaces;
using UserEntity = EClaim.Domain.Entities.User;
using SecurityClaim =  System.Security.Claims.Claim;
using ClaimTypes = System.Security.Claims.ClaimTypes;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EClaim.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }
    public (string token, DateTime expiresAt) GenerateToken(UserEntity user)
    {
        var expiresAt =  DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var claims = new[]
        {
            new SecurityClaim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new SecurityClaim(JwtRegisteredClaimNames.Email, user.Email),
            new SecurityClaim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new SecurityClaim(ClaimTypes.Name, user.FullName),
            new SecurityClaim(ClaimTypes.Role, user.Role.Name.ToString()),
            new SecurityClaim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
