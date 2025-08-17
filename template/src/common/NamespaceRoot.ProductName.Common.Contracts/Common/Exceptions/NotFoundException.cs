namespace NamespaceRoot.ProductName.Common.Contracts.Common.Exceptions;

/// <summary>
/// Возникает, если сущность не найдена в системе
/// </summary>
public class NotFoundException(string? message) : Exception(message)
{
    public NotFoundException(Type entityType, object id) : this($"Not Found {entityType} by id {id}")
    {
    }
}