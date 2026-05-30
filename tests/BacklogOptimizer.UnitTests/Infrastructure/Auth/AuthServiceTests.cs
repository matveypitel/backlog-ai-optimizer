using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Auth;
using BacklogOptimizer.Infrastructure.Persistence;
using BacklogOptimizer.UnitTests.Helpers;

using Moq;

namespace BacklogOptimizer.UnitTests.Infrastructure.Auth;

public class AuthServiceTests : IDisposable
{
    private readonly AuthService _authService;
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly ApplicationDbContext _dbContext;

    public AuthServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _authService = new AuthService(_dbContext, _hasherMock.Object, _tokenServiceMock.Object);

        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _tokenServiceMock.Setup(t => t.Generate(It.IsAny<User>()))
            .Returns(new AuthTokenResult("token", DateTime.UtcNow.AddHours(1), "test@example.com", "User"));
    }

    public void Dispose() => _dbContext.Dispose();

    // -- RegisterAsync --

    [Fact]
    public async Task RegisterAsync_ValidEmailAndPassword_ReturnsSuccess()
    {
        var result = await _authService.RegisterAsync("user@example.com", "password123");

        Assert.True(result.IsSuccess);
        Assert.Equal("token", result.Value!.AccessToken);
    }

    [Fact]
    public async Task RegisterAsync_EmptyEmail_ReturnsEmailRequiredError()
    {
        var result = await _authService.RegisterAsync("", "password123");

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.email_required", result.Error!.Id);
    }

    [Fact]
    public async Task RegisterAsync_WhitespaceEmail_ReturnsEmailRequiredError()
    {
        var result = await _authService.RegisterAsync("   ", "password123");

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.email_required", result.Error!.Id);
    }

    [Fact]
    public async Task RegisterAsync_ShortPassword_ReturnsPasswordTooShortError()
    {
        var result = await _authService.RegisterAsync("user@example.com", "short");

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.password_too_short", result.Error!.Id);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsEmailAlreadyExistsError()
    {
        await _authService.RegisterAsync("dup@example.com", "password123");

        var result = await _authService.RegisterAsync("DUP@example.com", "password456");

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.email_already_exists", result.Error!.Id);
    }

    // -- LoginAsync --

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        await _authService.RegisterAsync("login@example.com", "password123");

        var result = await _authService.LoginAsync("login@example.com", "password123");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task LoginAsync_EmptyEmail_ReturnsInvalidCredentials()
    {
        var result = await _authService.LoginAsync("", "password");

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.invalid_credentials", result.Error!.Id);
    }

    [Fact]
    public async Task LoginAsync_EmptyPassword_ReturnsInvalidCredentials()
    {
        var result = await _authService.LoginAsync("user@example.com", "");

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.invalid_credentials", result.Error!.Id);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsInvalidCredentials()
    {
        var result = await _authService.LoginAsync("nobody@example.com", "password123");

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.invalid_credentials", result.Error!.Id);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        await _authService.RegisterAsync("login2@example.com", "password123");
        _hasherMock.Setup(h => h.Verify("wrong", It.IsAny<string>())).Returns(false);

        var result = await _authService.LoginAsync("login2@example.com", "wrong");

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.invalid_credentials", result.Error!.Id);
    }

    // -- CreateUserAsync --

    [Fact]
    public async Task CreateUserAsync_ValidData_ReturnsCreatedUser()
    {
        var result = await _authService.CreateUserAsync("admin2@example.com", "password123", Role.Admin);

        Assert.True(result.IsSuccess);
        Assert.Equal("admin2@example.com", result.Value!.Email);
        Assert.Equal("Admin", result.Value.Role);
    }

    // -- GetUsersAsync --

    [Fact]
    public async Task GetUsersAsync_ReturnsAllUsers()
    {
        await _authService.RegisterAsync("a@example.com", "password123");
        await _authService.RegisterAsync("b@example.com", "password456");

        var users = await _authService.GetUsersAsync();

        Assert.Equal(2, users.Count);
    }

    // -- DeleteUserAsync --

    [Fact]
    public async Task DeleteUserAsync_CannotDeleteSelf()
    {
        var createResult = await _authService.CreateUserAsync("self@example.com", "password123", Role.User);
        var userId = createResult.Value!.Id;

        var result = await _authService.DeleteUserAsync(userId, userId);

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.cannot_delete_self", result.Error!.Id);
    }

    [Fact]
    public async Task DeleteUserAsync_UserNotFound_ReturnsNotFoundError()
    {
        var nonExistent = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var result = await _authService.DeleteUserAsync(nonExistent, requesterId);

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.user_not_found", result.Error!.Id);
    }

    [Fact]
    public async Task DeleteUserAsync_LastAdmin_ReturnsCannotDeleteLastAdminError()
    {
        var createResult = await _authService.CreateUserAsync("lastadmin@example.com", "password123", Role.Admin);
        var adminId = createResult.Value!.Id;
        var requesterId = Guid.NewGuid();

        var result = await _authService.DeleteUserAsync(adminId, requesterId);

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.cannot_delete_last_admin", result.Error!.Id);
    }

    [Fact]
    public async Task DeleteUserAsync_ValidUser_DeletesAndReturnsSuccess()
    {
        var adminResult = await _authService.CreateUserAsync("admin@example.com", "password123", Role.Admin);
        var userResult = await _authService.CreateUserAsync("user@example.com", "password123", Role.User);

        var result = await _authService.DeleteUserAsync(userResult.Value!.Id, adminResult.Value!.Id);

        Assert.True(result.IsSuccess);
        var users = await _authService.GetUsersAsync();
        Assert.Single(users);
    }
}
