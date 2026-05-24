using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
