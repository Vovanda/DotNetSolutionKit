using NamespaceRoot.ProductName.Common.Domain;

namespace NamespaceRoot.ProductName.Common.Application.Repositories;

/// <summary>
/// Представляет паттерн Unit of Work для работы с репозиториями и транзакциями.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Возвращает репозиторий для агрегатной сущности с указанным типом идентификатора.
    /// </summary>
    /// <typeparam name="TItem">Тип агрегатной сущности.</typeparam>
    /// <typeparam name="TId">Тип идентификатора.</typeparam>
    /// <returns>Репозиторий для работы с агрегатами.</returns>
    IRepository<TItem, TId> GetRepository<TItem, TId>() 
        where TItem : Entity<TId>, IAggregateRoot where TId : struct;

    /// <summary>
    /// Возвращает репозиторий только для чтения для сущности.
    /// </summary>
    /// <typeparam name="TItem">Тип сущности.</typeparam>
    /// <typeparam name="TId">Тип идентификатора.</typeparam>
    /// <returns>Репозиторий только для чтения.</returns>
    IReadOnlyRepository<TItem, TId> GetReadOnlyRepository<TItem, TId>() 
        where TItem : Entity;

    /// <summary>
    /// Сохраняет все изменения, сделанные через репозитории, в базу данных.
    /// </summary>
    Task SaveChangesAsync();

    /// <summary>
    /// Выполняет переданную функцию в рамках транзакции.
    /// Если транзакции нет, она будет создана автоматически.
    /// </summary>
    /// <typeparam name="TResult">Тип возвращаемого значения функции.</typeparam>
    /// <param name="func">Функция для выполнения в транзакции.</param>
    /// <returns>Результат выполнения функции.</returns>
    Task<TResult> RunInTransaction<TResult>(Func<IUnitOfWork, Task<TResult>> func);

    /// <summary>
    /// Выполняет переданный action в рамках транзакции.
    /// Если транзакции нет, она будет создана автоматически.
    /// </summary>
    /// <param name="func">Action для выполнения в транзакции.</param>
    Task RunInTransaction(Func<IUnitOfWork, Task> func);

    /// <summary>
    /// Возвращает список изменённых свойств сущности.
    /// Для новых сущностей возвращает все значимые значения.
    /// Для существующих — только изменившиеся.
    /// </summary>
    /// <param name="entity">Сущность для анализа изменений.</param>
    /// <param name="isNewEntity">Флаг, указывающий, что сущность новая.</param>
    /// <returns>Словарь с именем свойства, типом, старым и текущим значением.</returns>
    Dictionary<string, (Type Type, object? OriginalValue, object? CurrentValue)> GetChangesFor(object entity, bool isNewEntity = false);
}
