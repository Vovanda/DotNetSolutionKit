namespace NamespaceRoot.ProductName.Common.Exceptions;

/// <summary>
/// Occurs when rate limit is exceeded
/// </summary>
public class RateLimitException(string? message) : Exception(message)
{
    public RateLimitException(int limit, TimeSpan period) 
        : this($"Rate limit exceeded. Maximum {limit} requests per {period.TotalSeconds} seconds.")
    {
    }
    
    public RateLimitException(string resource, int limit, TimeSpan period) 
        : this($"Rate limit exceeded for {resource}. Maximum {limit} requests per {period.TotalSeconds} seconds.")
    {
    }
}