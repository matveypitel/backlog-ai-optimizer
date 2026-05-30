using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.UnitTests.Core.Common;

public class ResultTests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_IsSuccess_False_And_HasError()
    {
        var error = new Error("test.id", ErrorType.Validation, "desc");

        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_NullError_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        var error = new Error("test.id", ErrorType.NotFound, "not found");

        Result result = error;

        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }
}
