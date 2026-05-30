using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.UnitTests.Core.Common;

public class ResultOfTTests
{
    [Fact]
    public void Success_WithValue_IsSuccessTrue_ValueSet()
    {
        var result = Result<string>.Success("hello");

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_IsSuccessFalse_ValueNull()
    {
        var error = new Error("test.id", ErrorType.Validation, "bad");

        var result = Result<string>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        var error = new Error("e", ErrorType.NotFound, "msg");

        Result<int> result = error;

        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Success_NullableReferenceType_ValueIsNull()
    {
        var result = Result<string?>.Success(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }
}
