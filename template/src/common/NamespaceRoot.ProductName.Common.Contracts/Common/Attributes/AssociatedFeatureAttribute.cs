using NamespaceRoot.ProductName.Common.Contracts.FeatureManagement;

namespace NamespaceRoot.ProductName.Common.Contracts.Common.Attributes;

/// <summary>
/// Атрибут для пометки элемента, связанного с определённой фичей <see cref="FeatureFlags"/>.
/// </summary>
/// <param name="feature">Фича, с которой связано использование элемента.</param>
/// <remarks>
/// Обеспечивает контроль изменений при удалении фича-флага: удаление флага требует удаления атрибута, иначе сборка выдаст ошибку.  
/// Дополнительно можно использовать комментарии TODO или <see cref="ObsoleteAttribute"/> для генерации предупреждений.
/// </remarks>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class AssociatedFeatureAttribute(FeatureFlags feature) : Attribute
{
    /// <summary>
    /// Связанная фича.
    /// </summary>
    public FeatureFlags Feature { get; } = feature;
}