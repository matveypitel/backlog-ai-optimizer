using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.Application.Auth;

public interface ITokenService
{
    AuthTokenResult Generate(User user);
}
