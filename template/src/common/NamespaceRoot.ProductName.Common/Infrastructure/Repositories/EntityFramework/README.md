# Repository Layer

Эта папка содержит UnitOfWork и Generic реализацию репозиториев для работы с агрегатными сущностями через Entity Framework Core.

> Эта реализация UnitOfWork и базовых EF-репозиториев пока размещена в Common для удобства шаблона.

> !!! В идеале эти классы и интерфейсы следует вынести в отдельный Framework-проект
чтобы их можно было использовать независимо от конкретного микросервиса !!!

> Это позволит:
> - избежать зависимости всех сервисов от Common.Infrastructure, если им нужны только репозитории;
> - централизованно поддерживать универсальные реализации репозиториев и UoW;
> - упростить повторное использование и тестирование кода.


## Основные типы

- **RepositoryBase**  
  Базовый абстрактный класс для всех репозиториев, хранит `DbContext`.

- **GenericReadOnlyRepository<TEntity>**  
  Репозиторий только для чтения.  
  Реализует `IReadOnlyRepository<TEntity>` и может использоваться для любых сущностей, включая неагрегатные.

- **GenericRepository<TEntity>**  
  Абстрактный репозиторий для агрегатов (`IAggregateRoot`).  
  Реализует `IRepository<TEntity>` и `IReadOnlyRepository<TEntity>`.  
  Поддерживает добавление и удаление сущностей.

- **GenericRepository<TEntity, TId>**  
  Абстрактный репозиторий для агрегатов с идентификатором (`Entity<TId>`).  
  Реализует `IRepository<TEntity, TId>`, `IRepository<TEntity>` и `IReadOnlyRepository<TEntity>`.  
  Поддерживает `AddOrUpdateAsync`, поиск по Id и выборку с включением навигационных свойств.

- **QueryableExtensions.cs**  
  Расширения для `IQueryable`, включая поддержку `Include` / `ThenInclude` через функции `Func<IQueryable<TEntity>, IQueryable<TEntity>>`.

## Особенности

- Все методы `virtual` — можно переопределять в наследниках.  
- Асинхронные методы для всех CRUD операций.  
- Поддержка Include/ThenInclude через функцию `ApplyIncludes`.  
- Разделение ответственности:  
  - `GenericReadOnlyRepository` — доступ к данным (чтение).  
  - `GenericRepository` — работа с агрегатными корнями (`IAggregateRoot`).  
- Лёгкая интеграция с Unit of Work.  
- Совместимо с EF Core 5+ (тестировалось на 9.0.8).

## Пример использования

```csharp
var order = await orderRepository.FindOrThrow(
    orderId,
    q => q.Include(o => o.Customer)
          .ThenInclude(c => c.Address)
          .Include(o => o.Items)
          .ThenInclude(i => i.Product),
    cancellationToken);
````

> Все изменения в агрегатах должны сопровождаться сохранением через `DbContext.SaveChangesAsync()` в рамках Unit of Work.