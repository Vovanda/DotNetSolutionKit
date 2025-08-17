namespace NamespaceRoot.ProductName.Common.Domain;

/// <summary>
/// Базовое представление для сущности
/// </summary>
public abstract class Entity;

/// <summary>
/// Сущность с идентификатором
/// </summary>
/// <typeparam name="TId">Тип идентификатора</typeparam>
public abstract class Entity<TId> : Entity
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public TId Id { get; protected set; } = default!;
}