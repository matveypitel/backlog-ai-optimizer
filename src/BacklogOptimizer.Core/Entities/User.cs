using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Role Role { get; private set; }

    public User(string email, string passwordHash, Role role)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
