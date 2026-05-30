using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var user = new User("test@example.com", "hash123", Role.Admin);

        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("hash123", user.PasswordHash);
        Assert.Equal(Role.Admin, user.Role);
    }

    [Fact]
    public void Constructor_UserRole()
    {
        var user = new User("user@example.com", "hash", Role.User);

        Assert.Equal(Role.User, user.Role);
    }

    [Fact]
    public void UpdatePassword_ChangesHash()
    {
        var user = new User("test@example.com", "old-hash", Role.User);

        user.UpdatePassword("new-hash");

        Assert.Equal("new-hash", user.PasswordHash);
    }
}
