using JetBrains.Annotations;

namespace NamespaceRoot.ProductName.Common.Contracts.FeatureManagement;

/// <summary>
/// Имена флагов
/// </summary>
[PublicAPI]
public enum FeatureFlags
{
    /// <summary>
    /// Тестовый флаг
    /// К каждому фф добавляем номер pbi с описанием:
    /// PBI xxxxxx: Заголовок pbi
    /// </summary>
    TestFlag,

    /// <summary>
    /// Делает видимым тестовый контроллер сервиса
    /// </summary>
    AllowTestController
}
