namespace NamespaceRoot.ProductName.Common.Contracts.Responses;

/// <summary>
/// Base non-generic contract for all API envelopes.
/// Guarantees that the data field is always present for consistent JSON structure.
/// </summary>
public interface IApiEnvelope
{
    /// <summary>HTTP status code of the response (e.g., 200, 400, 500).</summary>
    int Status { get; }

    /// <summary>Machine-readable error code for programmatic handling (e.g., VALIDATION_ERROR).</summary>
    string? ErrorCode { get; }

    /// <summary>Human-readable message describing the result.</summary>
    string Message { get; }

    /// <summary>The data payload. Will be null in case of errors.</summary>
    object? Data { get; }

    /// <summary>Metadata containing pagination and additional business aggregates.</summary>
    EnvelopeMeta? Meta { get; }
}

/// <summary>
/// Generic contract for API envelopes with typed data payload.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
public interface IApiEnvelope<out T> : IApiEnvelope
{
    /// <summary>The typed data payload.</summary>
    new T? Data { get; }
}