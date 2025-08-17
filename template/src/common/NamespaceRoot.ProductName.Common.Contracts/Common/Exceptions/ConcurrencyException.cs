namespace NamespaceRoot.ProductName.Common.Contracts.Common.Exceptions;

/// <summary>
/// Ошибка конкурентного редактирования сущности
/// </summary>
public class ConcurrencyException : BusinessLogicException
{
    /// <summary>
    /// Ошибка конкурентного редактирования сущности
    /// </summary>
    /// <param name="message"></param>
    public ConcurrencyException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Ошибка конкурентного редактирования сущности
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public ConcurrencyException(string? message, Exception innerException) : base(message, innerException)
    {
    }
}
