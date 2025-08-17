namespace NamespaceRoot.ProductName.Common.Contracts.Common.Exceptions;

/// <summary>
/// Исключения уровня бизнес-логики
/// </summary>
public class BusinessLogicException: Exception
{
    /// <inheritdoc />
    public BusinessLogicException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    public BusinessLogicException(string? message, Exception innerException) : base(message, innerException)
    {
    }
}