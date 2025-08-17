using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NamespaceRoot.ProductName.Common.Domain;
using NamespaceRoot.ProductName.Common.Application.Repositories;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Repositories.EntityFramework;

// TODO: Эта реализация UnitOfWork `DbContextBase`  и базовых EF-репозиториев пока размещена в Common для удобства шаблона!
// NOTE: В идеале эти классы и интерфейсы следует вынести в отдельный Framework-проект,
// чтобы их можно было использовать независимо от конкретного микросервиса.
// Это позволит:
//  - избежать зависимости всех сервисов от Common.Infrastructure, если им нужны только репозитории;
//  - централизованно поддерживать универсальные реализации репозиториев и UoW;
//  - упростить повторное использование и тестирование кода.

/// <summary>
/// Репозиторий только для чтения.
/// Поддерживает асинхронные запросы с опциональным включением связанных сущностей.
/// </summary>
/// <typeparam name="TEntity">Тип сущности.</typeparam>
public abstract class GenericReadOnlyRepository<TEntity> : RepositoryBase, IReadOnlyRepository<TEntity> 
    where TEntity : Entity
{
    public virtual async Task<IEnumerable<TEntity>> QueryAllAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .ApplyIncludes(include)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(predicate)
            .ApplyIncludes(include)
            .ToListAsync(cancellationToken);
    }

    public virtual Task<TResult> QuerySingle<TResult>(
        Func<IQueryable<TEntity>, Task<TResult>> func)
    {
        return func(DbSet);
    }

    /// <summary>
    /// Выполняет запрос с фильтром и возвращает ограниченное количество элементов (batch).
    /// Include/ThenInclude применяются до фильтрации.
    /// </summary>
    /// <param name="predicate">Фильтр для сущностей.</param>
    /// <param name="batchSize">Количество элементов в пачке.</param>
    /// <param name="include">Функция для включения навигационных свойств.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список сущностей, удовлетворяющих условию, ограниченный batchSize.</returns>
    public virtual async Task<IEnumerable<TEntity>> BatchQueryAsync(
        Expression<Func<TEntity, bool>> predicate,
        int batchSize,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .ApplyIncludes(include)   // Навигационные свойства включаются до Where
            .Where(predicate)         // Применяем фильтр
            .Take(batchSize)          // Ограничиваем размер
            .ToListAsync(cancellationToken);
    }

    protected virtual IQueryable<TEntity> DbSet => Context.Set<TEntity>().AsSplitQuery();
}