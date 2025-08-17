using System.Linq.Expressions;
using NamespaceRoot.ProductName.Common.Domain;

namespace NamespaceRoot.ProductName.Common.Application.Repositories;

/// <summary>
/// Репозиторий только для чтения.
/// Поддерживает асинхронные запросы с опциональным включением связанных сущностей.
/// </summary>
/// <typeparam name="TEntity">Тип сущности.</typeparam>
public interface IReadOnlyRepository<TEntity>
    where TEntity : Entity
{
    /// <summary>
    /// Получает все сущности.
    /// </summary>
    /// <param name="include">Опциональная функция для включения связанных навигационных свойств (Include/ThenInclude).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IEnumerable<TEntity>> QueryAllAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает сущности, удовлетворяющие предикату.
    /// </summary>
    /// <param name="predicate">Фильтр сущностей.</param>
    /// <param name="include">Опциональная функция для включения связанных навигационных свойств.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IEnumerable<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет произвольный запрос на DbSet и возвращает результат.
    /// </summary>
    /// <typeparam name="TResult">Тип результата запроса.</typeparam>
    /// <param name="func">Функция с IQueryable для запроса.</param>
    Task<TResult> QuerySingle<TResult>(Func<IQueryable<TEntity>, Task<TResult>> func);
}

public interface IReadOnlyRepository<TEntity, in TId> : IReadOnlyRepository<TEntity>
    where TEntity : Entity
{
    /// <summary>
    /// Найдёт элемент по ID или выбросит исключение
    /// </summary>
    /// <param name="id">Идентификатор элемента</param>
    /// <param name="cancellationToken">Маркер отмены операции</param>
    /// <returns></returns>
    Task<TEntity> FindOrThrow(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Найдёт элемент по Id или вернёт значение по умолчанию
    /// </summary>
    /// <param name="id">Идентификатор элемента</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<TEntity?> FindOrDefault(TId id, CancellationToken cancellationToken = default);
}