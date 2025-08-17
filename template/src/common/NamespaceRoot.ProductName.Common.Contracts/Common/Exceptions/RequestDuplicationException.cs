namespace NamespaceRoot.ProductName.Common.Contracts.Common.Exceptions;

/// <summary>
/// Дубликат запроса на квотацию
/// </summary>
/// <param name="message"></param>
public class RequestDuplicationException(string? message) : BusinessLogicException(message);