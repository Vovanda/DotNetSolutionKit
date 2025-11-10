using JetBrains.Annotations;

namespace NamespaceRoot.ProductName.Common.Contracts.Responses;

/// <summary>
/// Specialized envelope for errors. 
/// Inherits from ApiEnvelope to ensure identical JSON structure and serialization behavior.
/// </summary>
[PublicAPI]
public class ErrorResponse : ApiEnvelope<object>
{
    public ErrorResponse(int status, string message, string? errorCode = null)
    {
        Status = status;
        Message = message;
        ErrorCode = errorCode;
        // Data is already null by default in the base class
    }
}