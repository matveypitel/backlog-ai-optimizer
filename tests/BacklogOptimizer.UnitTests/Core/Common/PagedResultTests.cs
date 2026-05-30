using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.UnitTests.Core.Common;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_RoundsUp()
    {
        var result = new PagedResult<int>([1, 2, 3], 1, 2, 5);

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void TotalPages_ExactDivision()
    {
        var result = new PagedResult<int>([1, 2], 1, 2, 4);

        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public void HasPrevious_False_OnFirstPage()
    {
        var result = new PagedResult<int>([], 1, 10, 20);

        Assert.False(result.HasPrevious);
    }

    [Fact]
    public void HasPrevious_True_OnSecondPage()
    {
        var result = new PagedResult<int>([], 2, 10, 20);

        Assert.True(result.HasPrevious);
    }

    [Fact]
    public void HasNext_False_OnLastPage()
    {
        var result = new PagedResult<int>([], 2, 10, 20);

        Assert.False(result.HasNext);
    }

    [Fact]
    public void HasNext_True_WhenMorePagesExist()
    {
        var result = new PagedResult<int>([], 1, 10, 20);

        Assert.True(result.HasNext);
    }

    [Fact]
    public void Items_AreReadOnly()
    {
        var result = new PagedResult<string>(["a", "b"], 1, 10, 2);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("a", result.Items[0]);
    }

    [Fact]
    public void Properties_AreSetCorrectly()
    {
        var result = new PagedResult<int>([1], 3, 5, 15);

        Assert.Equal(3, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(15, result.TotalCount);
    }
}
