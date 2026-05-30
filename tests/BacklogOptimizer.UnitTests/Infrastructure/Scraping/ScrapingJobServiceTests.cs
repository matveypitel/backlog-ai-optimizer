using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Infrastructure.Persistence;
using BacklogOptimizer.Infrastructure.Scraping;
using BacklogOptimizer.UnitTests.Helpers;

namespace BacklogOptimizer.UnitTests.Infrastructure.Scraping;

public class ScrapingJobServiceTests : IDisposable
{
    private readonly ScrapingJobService _service;
    private readonly ApplicationDbContext _dbContext;

    public ScrapingJobServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _service = new ScrapingJobService(_dbContext);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task EnqueueScrapeAsync_ValidUrl_ReturnsSuccess()
    {
        var result = await _service.EnqueueScrapeAsync("https://example.com");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task EnqueueScrapeAsync_ValidUrl_CreatesJobInDb()
    {
        await _service.EnqueueScrapeAsync("https://example.com/page");

        var job = _dbContext.ScrapingJobs.Single();
        Assert.Equal("https://example.com/page", job.Url);
    }

    [Fact]
    public async Task EnqueueScrapeAsync_EmptyUrl_ReturnsValidationError()
    {
        var result = await _service.EnqueueScrapeAsync("");

        Assert.False(result.IsSuccess);
        Assert.Equal("validation.url_required", result.Error!.Id);
    }

    [Fact]
    public async Task EnqueueScrapeAsync_WhitespaceUrl_ReturnsValidationError()
    {
        var result = await _service.EnqueueScrapeAsync("   ");

        Assert.False(result.IsSuccess);
        Assert.Equal("validation.url_required", result.Error!.Id);
    }

    [Fact]
    public async Task EnqueueScrapeAsync_RelativeUrl_ReturnsMalformedError()
    {
        var result = await _service.EnqueueScrapeAsync("/relative/path");

        Assert.False(result.IsSuccess);
        Assert.Equal("validation.url_malformed", result.Error!.Id);
    }

    [Fact]
    public async Task EnqueueScrapeAsync_MalformedUrl_ReturnsMalformedError()
    {
        var result = await _service.EnqueueScrapeAsync("not-a-url-at-all");

        Assert.False(result.IsSuccess);
        Assert.Equal("validation.url_malformed", result.Error!.Id);
    }

    [Fact]
    public async Task EnqueueScrapeAsync_InvalidUrl_DoesNotCreateJobInDb()
    {
        await _service.EnqueueScrapeAsync("bad-url");

        Assert.Empty(_dbContext.ScrapingJobs);
    }

    [Fact]
    public async Task EnqueueScrapeAsync_MultipleValidUrls_CreatesMultipleJobs()
    {
        await _service.EnqueueScrapeAsync("https://example.com/1");
        await _service.EnqueueScrapeAsync("https://example.com/2");

        Assert.Equal(2, _dbContext.ScrapingJobs.Count());
    }
}
