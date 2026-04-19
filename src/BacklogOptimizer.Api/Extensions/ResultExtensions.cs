using BacklogOptimizer.Core.Common;

using Microsoft.AspNetCore.Mvc;

namespace BacklogOptimizer.Api.Extensions;

internal static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, int successStatusCode = StatusCodes.Status200OK) =>
        result.IsSuccess
            ? new StatusCodeResult(successStatusCode)
            : ToProblem(result.Error!);

    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult>? onSuccess = null) =>
        result.IsSuccess
            ? onSuccess?.Invoke(result.Value!) ?? new OkObjectResult(result.Value)
            : ToProblem(result.Error!);

    private static IActionResult ToProblem(Error error)
    {
        var status = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(new ProblemDetails
        {
            Type = error.Id,
            Title = error.Type.ToString(),
            Detail = error.Description,
            Status = status
        })
        {
            StatusCode = status
        };
    }
}
