using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace Core.Common.HttpLogic;

public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    private const string RequestCanceledMessage = "Запрос был отменён";
    private const string InternalErrorMessage = "Упс! Что-то пошло не так.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        (httpContext.Response.StatusCode, var message) = exception switch
        {
            BusinessException ex => (StatusCodes.Status422UnprocessableEntity, ex.Message),
            TimeoutException ex => (StatusCodes.Status504GatewayTimeout, ex.Message),
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, RequestCanceledMessage),
            _ => (StatusCodes.Status500InternalServerError, InternalErrorMessage)
        };

        await httpContext.Response.WriteAsJsonAsync(new { error = message }, cancellationToken: cancellationToken);
        return true;
    }
}