namespace NamespaceRoot.ProductName.Common.Contracts.Domain.Context;

/// <summary>
/// Контекст выполнения любого действия в домене
/// </summary>
public interface IDomainExecutionContext
{
    /// <summary>
    /// Действующие лицо
    /// </summary>
    public IUserContext Actor { get; }

    /// <summary>
    /// Текущие дата, время
    /// </summary>
    public TimeProvider TimeProvider { get; }
}