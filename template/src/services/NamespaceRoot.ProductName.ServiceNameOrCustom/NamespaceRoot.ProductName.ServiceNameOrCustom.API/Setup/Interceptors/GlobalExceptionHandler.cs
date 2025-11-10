using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using NamespaceRoot.ProductName.Common.Contracts.Responses;
using NamespaceRoot.ProductName.Common.Exceptions;
using NamespaceRoot.ProductName.Common.Web.Misc;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Interceptors;

internal sealed class GlobalExceptionHandler(
    IWebHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        LogException(exception);

        // Map the exception to our unified ErrorResponse (which is an ApiEnvelope)
        var errorResponse = MapExceptionToError(exception);
        
        httpContext.Response.StatusCode = errorResponse.Status;
        httpContext.Response.ContentType = "application/json";

        // Since ErrorResponse inherits from ApiEnvelope, we always return a consistent structure.
        // Data property will be null in the JSON output.
        await httpContext.Response.WriteAsJsonAsync(errorResponse, JsonSetup.CommonOptions, cancellationToken);

        return true;
    }

    private void LogException(Exception exception)
    {
        switch (exception)
        {
            case BusinessLogicException:
                logger.LogWarning(exception, "Business logic violation occurred");
                break;
            case OperationCanceledException or TaskCanceledException:
                logger.LogInformation("Request was cancelled by the client or timed out");
                break;
            case JsonException:
                logger.LogWarning(exception, "Malformed JSON received from client");
                break;
            default:
                logger.LogError(exception, "An unhandled technical error occurred");
                break;
        }
    }

    private ErrorResponse MapExceptionToError(Exception exception)
    {
        return exception switch
        {
            OperationCanceledException or TaskCanceledException =>
                ErrorResponseHelper.CreateCustom(499, ErrorConstants.Messages.RequestCancelled, ErrorConstants.Codes.RequestCancelled),

            JsonException ex => ErrorResponseHelper.BadRequest($"{ErrorConstants.Messages.MalformedJson} {ex.Message}", ErrorConstants.Codes.BadRequest),

            SecurityTokenException ex => ErrorResponseHelper.Unauthorized(ex.Message, ErrorConstants.Codes.InvalidToken),

            UnauthorizedAccessException ex => ErrorResponseHelper.Unauthorized(ex.Message, ErrorConstants.Codes.Unauthorized),

            AccessDeniedException ex => ErrorResponseHelper.Forbidden(ex.Message, ex.ErrorCode ?? ErrorConstants.Codes.Forbidden),

            NotFoundException ex => ErrorResponseHelper.NotFound(ex.Message),

            ConcurrencyException ex => ErrorResponseHelper.Conflict(ex.Message, ex.ErrorCode ?? ErrorConstants.Codes.Conflict),

            ConflictException ex => ErrorResponseHelper.Conflict(ex.Message, ex.ErrorCode),

            BusinessLogicException ex => ErrorResponseHelper.UnprocessableEntity(ex.Message, ex.ErrorCode ?? ErrorConstants.Codes.ValidationError),

            ValidationException ex => ErrorResponseHelper.UnprocessableEntity(ex.Message),

            FluentValidation.ValidationException ex =>
                new ErrorResponse(400, ErrorConstants.Messages.ValidationFailed, ErrorConstants.Codes.ValidationError)
                {
                    Errors = ex.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray())
                },

            RateLimitException ex => ErrorResponseHelper.TooManyRequests(ex.Message),

            _ => environment.IsProduction()
                ? ErrorResponseHelper.InternalServerError()
                : ErrorResponseHelper.InternalServerError(exception.Message)
        };
    }
}