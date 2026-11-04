using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Core.Common.Errors;

public static class ResultExtensions
{
    private const string InternalErrorMessage = "Внутренняя ошибка сервера.";

    public static ActionResult<T> ToActionResult<T>(this Result<T> result) =>
        result.IsSuccess ? new OkObjectResult(result.Value) : ToProblem(result);

    public static ActionResult ToActionResult(this Result result) =>
        result.IsSuccess ? new NoContentResult() : ToProblem(result);

    public static ActionResult<TDto> ToActionResult<T, TDto>(this Result<T> result, Func<T, TDto> mapper) =>
        result.IsSuccess ? new OkObjectResult(mapper(result.Value)) : ToProblem(result);

    public static AppError GetAppError(this IResultBase failedResult) =>
        failedResult.Errors.OfType<AppError>().FirstOrDefault()
        ?? new AppError(ErrorStatus.Internal, InternalErrorMessage);

    private static ObjectResult ToProblem(IResultBase result)
    {
        var error = result.GetAppError();
        var status = error.Status.GetHttpResponseCode();
        ProblemDetails details = error is FieldValidationError validation
            ? new ValidationProblemDetails(validation.Fields)
            : new ProblemDetails();
        details.Status = status;
        details.Title = error.Status.ToString();
        details.Detail = error.Message;
        details.Type = $"https://httpstatuses.com/{status}";
        return new ObjectResult(details) { StatusCode = status };
    }
}
