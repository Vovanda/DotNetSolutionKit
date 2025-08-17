using NamespaceRoot.ProductName.Common.Domain;

namespace NamespaceRoot.ProductName.Common.Application.Repositories;

/// <summary>
/// Общий репозиторий для агрегатных сущностей.
/// Поддерживает добавление и удаление сущностей, а также чтение через IReadOnlyRepository.
/// </summary>
/// <typeparam name="TEntity">Тип агрегатной сущности.</typeparam>
public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>
    where TEntity : Entity, IAggregateRoot
{
    /// <summary>
    /// Асинхронно добавляет сущность в контекст.
    /// </summary>
    /// <param name="entity">Сущность для добавления.</param>
    ValueTask AddAsync(TEntity entity);

    /// <summary>
    /// Удаляет сущность из контекста.
    /// </summary>
    /// <param name="entity">Сущность для удаления.</param>
    void Remove(TEntity entity);
}

/// <summary>
/// Репозиторий для агрегатных сущностей с идентификатором.
/// Поддерживает добавление, обновление и поиск по Id.
/// </summary>
/// <typeparam name="TEntity">Тип агрегатной сущности.</typeparam>
/// <typeparam name="TId">Тип идентификатора сущности.</typeparam>
// ReSharper disable once TypeParameterCanBeVariant
public interface IRepository<TEntity, TId> : IRepository<TEntity>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : struct
{
    /// <summary>
    /// Добавляет сущность или обновляет её, если уже существует.
    /// </summary>
    /// <param name="entity">Сущность для добавления или обновления.</param>
    ValueTask AddOrUpdateAsync(TEntity entity);

    /// <summary>
    /// Находит сущность по Id или выбрасывает исключение, если не найдена.
    /// </summary>
    /// <param name="id">Идентификатор сущности.</param>
    /// <param name="include">Опциональная функция для включения связанных навигационных свойств.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<TEntity> FindOrThrow(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Находит сущность по Id или возвращает null, если не найдена.
    /// </summary>
    /// <param name="id">Идентификатор сущности.</param>
    /// <param name="include">Опциональная функция для включения связанных навигационных свойств.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<TEntity?> FindOrDefault(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);
}