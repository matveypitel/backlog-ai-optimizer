using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Infrastructure.Auth;

internal sealed class AuthService : IAuthService
{
    private const int MinPasswordLength = 8;

    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthTokenResult>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return Errors.Auth.InvalidCredentials;

        var normalized = email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);

        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
            return Errors.Auth.InvalidCredentials;

        return _tokenService.Generate(user);
    }

    public async Task<Result<AuthTokenResult>> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var creation = await CreateUserInternalAsync(email, password, Role.User, cancellationToken);
        if (!creation.IsSuccess)
            return creation.Error!;

        var user = await _dbContext.Users
            .FirstAsync(u => u.Id == creation.Value!.Id, cancellationToken);

        return _tokenService.Generate(user);
    }

    public async Task<Result<CreatedUserResult>> CreateUserAsync(string email, string password, Role role, CancellationToken cancellationToken = default) =>
        await CreateUserInternalAsync(email, password, role, cancellationToken);

    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserSummary(u.Id, u.Email, u.Role.ToString(), u.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    private async Task<Result<CreatedUserResult>> CreateUserInternalAsync(string email, string password, Role role, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Errors.Auth.EmailRequired;

        if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            return Errors.Auth.PasswordTooShort;

        var normalized = email.Trim().ToLowerInvariant();

        var exists = await _dbContext.Users
            .AnyAsync(u => u.Email == normalized, cancellationToken);

        if (exists)
            return Errors.Auth.EmailAlreadyExists(normalized);

        var user = new User(normalized, _passwordHasher.Hash(password), role);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreatedUserResult(user.Id, user.Email, user.Role.ToString());
    }
}
