using JetBrains.Annotations;

namespace NamespaceRoot.ProductName.Common.Contracts.Common.Lookups;

/// <summary>
/// Ответ на поиск данных в реестре сервиса (универсальный)
/// </summary>
[PublicAPI]
public sealed record LookupResponse : LookupResponse<object>
{
    public LookupResponse(List<LookupItem<object>> items, int totalCount) 
        : base(items, totalCount) { }
}

/// <summary>
/// Ответ на поиск данных в реестре сервиса
/// </summary>
/// <typeparam name="TInfo">Тип дополнительной информации</typeparam>
[PublicAPI]
public record LookupResponse<TInfo>(List<LookupItem<TInfo>> Items, int TotalCount);

/// <summary>
/// Вариант подсказки
/// </summary>
/// <typeparam name="TInfo">Тип дополнительной информации</typeparam>
[PublicAPI]
public record LookupItem<TInfo>
{
    /// <summary>
    /// Текст подсказки
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Дополнительная информация
    /// </summary>
    public TInfo? Info { get; init; }
}