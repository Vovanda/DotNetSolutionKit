using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NamespaceRoot.ProductName.Common.Contracts.Common.Exceptions;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Interceptors;

[UsedImplicitly]
public sealed class ExceptionFilter(IWebHostEnvironment environment, ILogger<ExceptionFilter> logger) : IExceptionFilter
{
    /// <summary>
    /// Маппинг исключений в http-коды ответа.
    /// </summary>
    public void OnException(ExceptionContext context)
    {
        if (context is { Exception: OperationCanceledException, HttpContext.RequestAborted.IsCancellationRequested: true })
        {
            context.ExceptionHandled = true;
            context.Result = new StatusCodeResult(statusCode: StatusCodes.Status499ClientClosedRequest);
            return;
        }

        logger.LogError(context.Exception, "Произошла неожиданная ошибка при обработке запроса");
        context.ExceptionHandled = true;
        context.Result = context.Exception switch
        {
            AccessDeniedException ex => new ObjectResult(ex) { StatusCode = StatusCodes.Status403Forbidden, Value = ex.Message },
            UnauthorizedAccessException ex => new ObjectResult(ex) { StatusCode = StatusCodes.Status401Unauthorized, Value = ex.Message },
            NotFoundException ex => new ObjectResult(ex) { StatusCode = StatusCodes.Status404NotFound, Value = ex.Message },
            ConcurrencyException ex => new ObjectResult(ex) { StatusCode = StatusCodes.Status409Conflict, Value = ex.Message },
            ConflictException ex => new ObjectResult(ex) { StatusCode = StatusCodes.Status409Conflict, Value = ex.Message },
            BusinessLogicException ex => new ObjectResult(ex) { StatusCode = StatusCodes.Status400BadRequest, Value = ex.Message },
            _ => new ObjectResult(context.Exception)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Value = environment.IsProduction() ? null : context.Exception.Message
            }
        };
    }
}
