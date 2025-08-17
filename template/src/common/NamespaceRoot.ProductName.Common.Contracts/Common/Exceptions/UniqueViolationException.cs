namespace NamespaceRoot.ProductName.Common.Contracts.Common.Exceptions;

/// <summary>
/// Возникнет если будет выявлено нарушение ограничения на уникальность
/// </summary>
/// <param name="message"></param>
public sealed class UniqueViolationException(string? message) : BusinessLogicException(message);