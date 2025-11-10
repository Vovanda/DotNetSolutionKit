using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace NamespaceRoot.ProductName.Common.Contracts.Responses;

/// <summary>
/// Standard implementation of the API response envelope.
/// Provides a consistent structure for both successful and error responses.
/// </summary>
/// <typeparam name="T">The type of the data being returned.</typeparam>
[PublicAPI]
public class ApiEnvelope<T> : IApiEnvelope<T>
{
    public int Status { get; init; }
    public string? ErrorCode { get; init; }
    public string Message { get; init; } = string.Empty;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public T? Data { get; init; }
    
    object? IApiEnvelope.Data => Data;

    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
    public EnvelopeMeta? Meta { get; set; }
}

/// <summary>
/// Represents metadata for the response, including pagination and dynamic business fields.
/// </summary>
[PublicAPI]
public class EnvelopeMeta : IPaginationResponse
{
    /// <inheritdoc />
    public int Page { get; init; }

    /// <inheritdoc />
    public int PageSize { get; init; }

    /// <inheritdoc />
    public int TotalCount { get; init; }

    /// <inheritdoc />
    public int TotalPages { get; init; }

    /// <summary>
    /// Explicit dictionary for extra business fields and aggregates not present in the standard contract (e.g., TotalActive).
    /// </summary>
    public Dictionary<string, object?>? Extra { get; init; }
}