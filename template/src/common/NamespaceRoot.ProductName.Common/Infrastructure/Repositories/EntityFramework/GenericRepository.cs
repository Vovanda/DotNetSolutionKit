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

public abstract class GenericRepository<TEntity> : GenericReadOnlyRepository<TEntity>, IRepository<TEntity>
    where TEntity : Entity, IAggregateRoot
{
    public virtual async ValueTask AddAsync(TEntity entity)
    {
        await Context.AddAsync(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        Context.Remove(entity);
    }
}

public abstract class GenericRepository<TEntity, TId> : GenericRepository<TEntity>, IRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : struct
{
    public virtual async ValueTask AddOrUpdateAsync(TEntity entity)
    {
        var isExist = await Context.Set<TEntity>().AnyAsync(x => x.Id.Equals(entity.Id));
        if (!isExist)
        {
            await Context.AddAsync(entity);
        }
        else
        {
            Context.Update(entity);
        }
    }
    
    public virtual Task<TEntity> FindOrThrow(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .ApplyIncludes(include)
            .SingleAsync(x => x.Id.Equals(id), cancellationToken);
    }

    public virtual Task<TEntity?> FindOrDefault(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .ApplyIncludes(include)
            .SingleOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
    }
}

