using BacklogOptimizer.Application.Scraping;
using BacklogOptimizer.Infrastructure.Persistence;
using BacklogOptimizer.Infrastructure.Scraping;
using BacklogOptimizer.UnitTests.Helpers;

namespace BacklogOptimizer.UnitTests.Infrastructure.Scraping;

public class ScrapingSourceServiceTests : IDisposable
{
    private readonly ScrapingSourceService _service;
    private readonly ApplicationDbContext _dbContext;

    public ScrapingSourceServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _service = new ScrapingSourceService(_dbContext);
    }

    public void Dispose() => _dbContext.Dispose();

    // -- GetAllAsync --

    [Fact]
    public async Task GetAllAsync_NoSources_ReturnsEmpty()
    {
        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSources()
    {
        await _service.CreateAsync(new CreateScrapingSourceCommand("https://example.com", "Example", null));

        var result = await _service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("https://example.com", result[0].Url);
    }

    // -- CreateAsync --

    [Fact]
    public async Task CreateAsync_ValidUrl_ReturnsSuccessWithDto()
    {
        var result = await _service.CreateAsync(
            new CreateScrapingSourceCommand("https://example.com", "Example", 24));

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com", result.Value!.Url);
        Assert.Equal("Example", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.Equal(24, result.Value.RefreshIntervalHours);
    }

    [Fact]
    public async Task CreateAsync_EmptyUrl_ReturnsUrlRequiredError()
    {
        var result = await _service.CreateAsync(new CreateScrapingSourceCommand("", null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal("validation.url_required", result.Error!.Id);
    }

    [Fact]
    public async Task CreateAsync_WhitespaceUrl_ReturnsUrlRequiredError()
    {
        var result = await _service.CreateAsync(new CreateScrapingSourceCommand("   ", null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal("validation.url_required", result.Error!.Id);
    }

    [Fact]
    public async Task CreateAsync_RelativeUrl_ReturnsMalformedError()
    {
        var result = await _service.CreateAsync(new CreateScrapingSourceCommand("/relative", null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal("validation.url_malformed", result.Error!.Id);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUrl_ReturnsConflictError()
    {
        await _service.CreateAsync(new CreateScrapingSourceCommand("https://example.com", null, null));

        var result = await _service.CreateAsync(new CreateScrapingSourceCommand("https://example.com", null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal("scraping_source.url_already_exists", result.Error!.Id);
    }

    // -- UpdateAsync --

    [Fact]
    public async Task UpdateAsync_ExistingSource_UpdatesAndReturnsDto()
    {
        var created = await _service.CreateAsync(new CreateScrapingSourceCommand("https://example.com", "Old Name", null));

        var result = await _service.UpdateAsync(created.Value!.Id,
            new UpdateScrapingSourceCommand("New Name", false, 48));

        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value!.Name);
        Assert.False(result.Value.IsActive);
        Assert.Equal(48, result.Value.RefreshIntervalHours);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNotFoundError()
    {
        var result = await _service.UpdateAsync(Guid.NewGuid(),
            new UpdateScrapingSourceCommand("Name", true, null));

        Assert.False(result.IsSuccess);
        Assert.Equal("scraping_source.not_found", result.Error!.Id);
    }

    // -- DeleteAsync --

    [Fact]
    public async Task DeleteAsync_ExistingSource_DeletesAndReturnsSuccess()
    {
        var created = await _service.CreateAsync(new CreateScrapingSourceCommand("https://example.com", null, null));

        var result = await _service.DeleteAsync(created.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(await _service.GetAllAsync());
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsNotFoundError()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal("scraping_source.not_found", result.Error!.Id);
    }

    // -- SyncAllActiveAsync --

    [Fact]
    public async Task SyncAllActiveAsync_NoSources_ReturnsZeroEnqueued()
    {
        var result = await _service.SyncAllActiveAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.Enqueued);
        Assert.Equal(0, result.Value.Skipped);
    }

    [Fact]
    public async Task SyncAllActiveAsync_ActiveSource_EnqueuesJob()
    {
        await _service.CreateAsync(new CreateScrapingSourceCommand("https://example.com", null, null));

        var result = await _service.SyncAllActiveAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Enqueued);
        Assert.Equal(0, result.Value.Skipped);
    }

    [Fact]
    public async Task SyncAllActiveAsync_InactiveSource_NotEnqueued()
    {
        var created = await _service.CreateAsync(new CreateScrapingSourceCommand("https://example.com", null, null));
        await _service.UpdateAsync(created.Value!.Id, new UpdateScrapingSourceCommand(null, false, null));

        var result = await _service.SyncAllActiveAsync();

        Assert.Equal(0, result.Value!.Enqueued);
    }

    [Fact]
    public async Task SyncAllActiveAsync_AlreadyInFlight_SkipsSource()
    {
        await _service.CreateAsync(new CreateScrapingSourceCommand("https://example.com", null, null));

        await _service.SyncAllActiveAsync();
        var result = await _service.SyncAllActiveAsync();

        Assert.Equal(0, result.Value!.Enqueued);
        Assert.Equal(1, result.Value.Skipped);
    }

    // -- SyncOneAsync --

    [Fact]
    public async Task SyncOneAsync_ExistingSource_CreatesJob()
    {
        var created = await _service.CreateAsync(new CreateScrapingSourceCommand("https://example.com", null, null));

        var result = await _service.SyncOneAsync(created.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, _dbContext.ScrapingJobs.Count());
    }

    [Fact]
    public async Task SyncOneAsync_NotFound_ReturnsNotFoundError()
    {
        var result = await _service.SyncOneAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal("scraping_source.not_found", result.Error!.Id);
    }

    [Fact]
    public async Task SyncOneAsync_AlreadyInFlight_SkipsAndReturnsSuccess()
    {
        var created = await _service.CreateAsync(new CreateScrapingSourceCommand("https://example.com", null, null));
        await _service.SyncOneAsync(created.Value!.Id);

        var result = await _service.SyncOneAsync(created.Value.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, _dbContext.ScrapingJobs.Count());
    }
}
