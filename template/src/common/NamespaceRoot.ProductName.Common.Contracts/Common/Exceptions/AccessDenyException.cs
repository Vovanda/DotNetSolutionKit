namespace NamespaceRoot.ProductName.Common.Contracts.Common.Exceptions;

/// <summary>
/// Возникает, если у текущего пользователя нет доступа к ресурсу
/// </summary>
public sealed class AccessDeniedException(string? message) : BusinessLogicException(message);