using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.Application.Auth;

public interface IAuthService
{
    Task<Result<AuthTokenResult>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<Result<AuthTokenResult>> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<Result<CreatedUserResult>> CreateUserAsync(string email, string password, Role role, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default);
}
