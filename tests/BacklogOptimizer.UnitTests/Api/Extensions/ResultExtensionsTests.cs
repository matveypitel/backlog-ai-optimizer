using BacklogOptimizer.Api.Extensions;
using BacklogOptimizer.Core.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BacklogOptimizer.UnitTests.Api.Extensions;

public class ResultExtensionsTests
{
    // -- Result (non-generic) --

    [Fact]
    public void ToActionResult_SuccessResult_ReturnsOk()
    {
        var result = Result.Success();

        var actionResult = result.ToActionResult();

        var statusResult = Assert.IsType<StatusCodeResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_SuccessWithCustomCode_ReturnsCustomStatusCode()
    {
        var result = Result.Success();

        var actionResult = result.ToActionResult(StatusCodes.Status204NoContent);

        var statusResult = Assert.IsType<StatusCodeResult>(actionResult);
        Assert.Equal(StatusCodes.Status204NoContent, statusResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_ValidationError_Returns400()
    {
        var result = Result.Failure(new Error("err.id", ErrorType.Validation, "bad input"));

        var actionResult = result.ToActionResult();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("err.id", problem.Type);
        Assert.Equal("bad input", problem.Detail);
    }

    [Fact]
    public void ToActionResult_NotFoundError_Returns404()
    {
        var result = Result.Failure(new Error("nf.id", ErrorType.NotFound, "not found"));

        var actionResult = result.ToActionResult();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_UnauthorizedError_Returns401()
    {
        var result = Result.Failure(new Error("auth.id", ErrorType.Unauthorized, "unauthorized"));

        var actionResult = result.ToActionResult();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_ConflictError_Returns409()
    {
        var result = Result.Failure(new Error("conf.id", ErrorType.Conflict, "conflict"));

        var actionResult = result.ToActionResult();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
    }

    // -- Result<T> --

    [Fact]
    public void ToActionResult_GenericSuccessWithValue_ReturnsOkObjectResult()
    {
        var result = Result<string>.Success("hello");

        var actionResult = result.ToActionResult();

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("hello", okResult.Value);
    }

    [Fact]
    public void ToActionResult_GenericSuccessWithOnSuccess_CallsOnSuccess()
    {
        var result = Result<int>.Success(42);
        var called = false;

        var actionResult = result.ToActionResult(v =>
        {
            called = true;
            return new OkObjectResult(v * 2);
        });

        Assert.True(called);
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(84, okResult.Value);
    }

    [Fact]
    public void ToActionResult_GenericFailure_ReturnsProblemDetails()
    {
        var result = Result<string>.Failure(new Error("err.id", ErrorType.NotFound, "not found"));

        var actionResult = result.ToActionResult();

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
    }

    [Fact]
    public void ToActionResult_GenericSuccessWithNullOnSuccess_UsesDefaultOkResult()
    {
        var result = Result<string>.Success("value");

        var actionResult = result.ToActionResult(onSuccess: null);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("value", okResult.Value);
    }
}
