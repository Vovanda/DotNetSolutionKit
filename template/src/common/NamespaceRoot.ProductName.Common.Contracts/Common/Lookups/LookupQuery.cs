using JetBrains.Annotations;

namespace NamespaceRoot.ProductName.Common.Contracts.Common.Lookups;

/// <summary>
/// Запрос на поиск данных в сервисе
/// </summary>
[PublicAPI]
public sealed record LookupQuery(string? Search);