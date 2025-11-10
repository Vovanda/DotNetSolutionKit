namespace NamespaceRoot.ProductName.Common.Exceptions;

/// <summary>
/// Thrown when a unique constraint violation occurs, e.g., duplicate entity.
/// </summary>
public sealed class UniqueViolationException : BusinessLogicException
{
    /// <summary>
    /// Initializes a new instance of <see cref="UniqueViolationException"/> with a specified error message.
    /// </summary>
    /// <param name="message">The error message describing the unique constraint violation.</param>
    public UniqueViolationException(string? message = null) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="UniqueViolationException"/> with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message describing the unique constraint violation.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public UniqueViolationException(string? message, Exception innerException) : base(message, innerException) { }
}