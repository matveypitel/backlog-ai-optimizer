using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;
using BacklogOptimizer.UnitTests.Helpers;

namespace BacklogOptimizer.UnitTests.Infrastructure.Persistence;

public class ApplicationDbContextTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public ApplicationDbContextTests()
    {
        _dbContext = TestDbContextFactory.Create();
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task SaveChangesAsync_NewEntity_SetsCreatedAt()
    {
        var before = DateTime.UtcNow;
        var job = new ScrapingJob("https://example.com");

        _dbContext.ScrapingJobs.Add(job);
        await _dbContext.SaveChangesAsync();

        Assert.True(job.CreatedAt >= before);
        Assert.Null(job.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedEntity_SetsUpdatedAt()
    {
        var job = new ScrapingJob("https://example.com");
        _dbContext.ScrapingJobs.Add(job);
        await _dbContext.SaveChangesAsync();

        var before = DateTime.UtcNow;
        job.MarkRunning();
        await _dbContext.SaveChangesAsync();

        Assert.NotNull(job.UpdatedAt);
        Assert.True(job.UpdatedAt >= before);
    }

    [Fact]
    public async Task SaveChangesAsync_NewEntity_CreatedAtMatchesNow()
    {
        var user = new User("time@example.com", "hash", Role.User);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.True(user.CreatedAt > DateTime.UtcNow.AddSeconds(-5));
    }
}
