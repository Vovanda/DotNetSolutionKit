namespace NamespaceRoot.ProductName.Common.Contracts.Common.Exceptions;

/// <summary>
/// Возникает, если возникла конфликтная ситуация
/// </summary>
public sealed class ConflictException: BusinessLogicException
{
    /// <inheritdoc />
    public ConflictException(string? message) : base(message)
    {
    }
}