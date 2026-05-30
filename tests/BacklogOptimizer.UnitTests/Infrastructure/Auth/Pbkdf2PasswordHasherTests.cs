using BacklogOptimizer.Infrastructure.Auth;

namespace BacklogOptimizer.UnitTests.Infrastructure.Auth;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = _hasher.Hash("MyPassword123");

        Assert.NotEmpty(hash);
    }

    [Fact]
    public void Hash_TwoCalls_ReturnDifferentHashes()
    {
        var hash1 = _hasher.Hash("password");
        var hash2 = _hasher.Hash("password");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Hash_HasThreeParts_SeparatedByDot()
    {
        var hash = _hasher.Hash("password");

        var parts = hash.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("correct-password");

        Assert.True(_hasher.Verify("correct-password", hash));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("correct-password");

        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Verify_MalformedHash_ReturnsFalse()
    {
        Assert.False(_hasher.Verify("password", "not-a-valid-hash"));
    }

    [Fact]
    public void Verify_TooFewParts_ReturnsFalse()
    {
        Assert.False(_hasher.Verify("password", "only.two"));
    }

    [Fact]
    public void Verify_NonNumericIterations_ReturnsFalse()
    {
        Assert.False(_hasher.Verify("password", "notanumber.salt.key"));
    }

    [Fact]
    public void Hash_SpecialCharacters_VerifiesCorrectly()
    {
        const string password = "P@$$w0rd!#%^&*()";
        var hash = _hasher.Hash(password);

        Assert.True(_hasher.Verify(password, hash));
    }
}
