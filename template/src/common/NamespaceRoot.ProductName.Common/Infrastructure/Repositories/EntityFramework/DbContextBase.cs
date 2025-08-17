using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
/// Базовый DbContext с поддержкой UnitOfWork и Generic репозиториев.
/// Позволяет переопределять методы для специфической логики микросервиса.
/// </summary>
public abstract class DbContextBase(DbContextOptions options, IServiceProvider serviceProvider)
    : DbContext(options), IUnitOfWork
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    #region IRepository/IReadOnlyRepository

    public virtual IRepository<TItem, TId> GetRepository<TItem, TId>()
        where TItem : Entity<TId>, IAggregateRoot
        where TId : struct
    {
        var repo = _serviceProvider.GetRequiredService<IRepository<TItem, TId>>();
        if (repo is RepositoryBase baseRepo)
        {
            baseRepo.SetContext(this);
        }
        return repo;
    }

    public virtual IReadOnlyRepository<TItem, TId> GetReadOnlyRepository<TItem, TId>()
        where TItem : Entity
    {
        var repo = _serviceProvider.GetRequiredService<IReadOnlyRepository<TItem, TId>>();
        if (repo is RepositoryBase baseRepo)
        {
            baseRepo.SetContext(this);
        }
        return repo;
    }

    #endregion

    #region SaveChanges и транзакции

    public virtual async Task SaveChangesAsync()
    {
        await base.SaveChangesAsync();
    }

    public virtual async Task<TResult> RunInTransaction<TResult>(Func<IUnitOfWork, Task<TResult>> func)
    {
        TResult result;

        if (Database.CurrentTransaction != null)
        {
            result = await func(this);
        }
        else
        {
            await using var transaction = await Database.BeginTransactionAsync();
            try
            {
                result = await func(this);
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        return result;
    }

    public virtual async Task RunInTransaction(Func<IUnitOfWork, Task> func)
    {
        await RunInTransaction(async uow =>
        {
            await func(uow);
            return true;
        });
    }

    #endregion

    #region GetChangesFor

    public Dictionary<string, (Type Type, object? OriginalValue, object? CurrentValue)> GetChangesFor(object entity, bool isNewEntity = false)
    {
        var trackedEntry = ChangeTracker.Entries().First(x => x.Entity == entity);
        var result = new Dictionary<string, (Type, object?, object?)>();

        foreach (var property in trackedEntry.Properties.OrderBy(p => p.Metadata.Name))
        {
            if (property.Metadata.IsShadowProperty())
                continue;

            var original = property.OriginalValue;
            var current = property.CurrentValue;

            if (isNewEntity)
            {
                // Для новых сущностей учитываем только значимые значения
                if (current is null ||
                    current is false ||
                    (current is string s && string.IsNullOrEmpty(s)) ||
                    (current is DateTimeOffset dto && dto == default))
                {
                    continue;
                }
                result[property.Metadata.Name] = (property.Metadata.ClrType, null, current);
            }
            else
            {
                // Для существующих — только изменившиеся
                if (!Equals(original, current))
                {
                    result[property.Metadata.Name] = (property.Metadata.ClrType, original, current);
                }
            }
        }

        return result;
    }
    #endregion
}
