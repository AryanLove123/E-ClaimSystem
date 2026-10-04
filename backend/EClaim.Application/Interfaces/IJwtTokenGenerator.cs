using EClaim.Domain.Entities;

namespace EClaim.Application.Interfaces;
public interface IJwtTokenGenerator
{
    (string token, DateTime expiresAt) GenerateToken(User user);
}