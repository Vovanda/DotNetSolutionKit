using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using NamespaceRoot.ProductName.Common.Contracts.Responses;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NamespaceRoot.ProductName.Common.Web.Misc;

/// <summary>
/// Helper for creating ErrorResponse instances with default messages
/// </summary>
[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class ErrorResponseHelper
{
    // Default error messages
    public static class Messages
    {
        public const string BadRequest = "The request is invalid.";
        public const string Unauthorized = "ServiceNameOrCustomentication required. Please provide valid credentials.";
        public const string Forbidden = "Access denied. You don't have permission to access this resource.";
        public const string NotFound = "The requested resource was not found.";
        public const string Conflict = "The request conflicts with the current state of the server.";
        public const string ValidationFailed = "One or more validation errors occurred.";
        public const string TooManyRequests = "Too many requests. Please try again later.";
        public const string InternalServerError = "An internal server error occurred.";
        public const string ServiceUnavailable = "Service temporarily unavailable.";
    }

    // Error codes
    public static class Codes
    {
        public const string BadRequest = "BAD_REQUEST";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string NotFound = "NOT_FOUND";
        public const string Conflict = "CONFLICT";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string RateLimit = "RATE_LIMIT";
        public const string InternalError = "INTERNAL_ERROR";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    }

    /// <summary>
    /// Creates a detailed ErrorResponse for validation failures using the provided naming policy.
    /// </summary>
    /// <param name="modelState">The ModelState containing validation errors.</param>
    /// <param name="namingPolicy">The JSON naming policy to apply to field names (keys).</param>
    /// <returns>A populated ErrorResponse with field-specific errors in the specified naming style.</returns>
    public static ErrorResponse ValidationError(
        ModelStateDictionary modelState, 
        JsonNamingPolicy? namingPolicy)
    {
        var validationErrors = modelState
            .Where(m => m.Value != null && m.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => namingPolicy != null 
                    ? namingPolicy.ConvertName(kvp.Key) 
                    : kvp.Key, 
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        return new ErrorResponse(400, "One or more validation errors occurred.", "VALIDATION_ERROR")
        {
            Errors = validationErrors
        };
    }
    
    public static ErrorResponse BadRequest(string? message = null, string? errorCode = null)
        => new(400, message ?? Messages.BadRequest, errorCode ?? Codes.BadRequest);

    public static ErrorResponse Unauthorized(string? message = null, string? errorCode = null)
        => new(401, message ?? Messages.Unauthorized, errorCode ?? Codes.Unauthorized);

    public static ErrorResponse Forbidden(string? message = null, string? errorCode = null)
        => new(403, message ?? Messages.Forbidden, errorCode ?? Codes.Forbidden);

    public static ErrorResponse NotFound(string? message = null, string? errorCode = null)
        => new(404, message ?? Messages.NotFound, errorCode ?? Codes.NotFound);

    public static ErrorResponse Conflict(string? message = null, string? errorCode = null)
        => new(409, message ?? Messages.Conflict, errorCode ?? Codes.Conflict);

    public static ErrorResponse UnprocessableEntity(string? message = null, string? errorCode = null)
        => new(422, message ?? Messages.ValidationFailed, errorCode ?? Codes.ValidationError);

    public static ErrorResponse TooManyRequests(string? message = null, string? errorCode = null)
        => new(429, message ?? Messages.TooManyRequests, errorCode ?? Codes.RateLimit);

    public static ErrorResponse InternalServerError(string? message = null, string? errorCode = null)
        => new(500, message ?? Messages.InternalServerError, errorCode ?? Codes.InternalError);

    public static ErrorResponse ServiceUnavailable(string? message = null, string? errorCode = null)
        => new(503, message ?? Messages.ServiceUnavailable, errorCode ?? Codes.ServiceUnavailable);
    
    /// <summary>
    /// Creates a custom error response for non-standard HTTP status codes
    /// </summary>
    public static ErrorResponse CreateCustom(int statusCode, string message, string? errorCode = null)
        => new(statusCode, message, errorCode);
}