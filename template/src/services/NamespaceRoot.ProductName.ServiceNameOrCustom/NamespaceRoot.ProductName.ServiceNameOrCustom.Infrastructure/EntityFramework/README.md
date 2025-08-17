# Работа с данными (UoW + репозитории)

## 1. Unit of Work и DbContext

`DbContextBase` — это базовый `DbContext`, который **реализует `IUnitOfWork`**.  
Он обеспечивает работу с репозиториями, транзакциями и отслеживание изменений сущностей.

### Возможности

* Получение репозиториев:
  ```csharp
  var repo = uow.GetRepository<OrderEntity, int>();
  var readOnlyRepo = uow.GetReadOnlyRepository<ProductEntity, int>();
  ```

* Сохранение изменений:

  ```csharp
  await uow.SaveChangesAsync();
  ```
* Выполнение транзакций:

  ```csharp
  await uow.RunInTransaction(async u =>
  {
      var order = await u.GetRepository<OrderEntity, int>().FindOrThrow(orderId);
      order.Status = OrderStatus.Paid;
      await u.SaveChangesAsync();
  });
  ```
* Получение изменённых свойств сущности:

  ```csharp
  var changes = uow.GetChangesFor(order);
  ```

### Пример микросервисного `DbContext`

`StoreDbContext` наследует `DbContextBase` и может переопределять методы, добавлять конфигурации схемы и моделей:

```csharp
public class StoreDbContext : DbContextBase
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options, IServiceProvider serviceProvider)
        : base(options, serviceProvider) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Схема по умолчанию
        modelBuilder.HasDefaultSchema("tmp");

        // Автоматическая регистрация всех конфигураций из сборки
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
```

> Все вызовы `GetRepository`/`GetReadOnlyRepository`, `SaveChangesAsync` и транзакции через `DbContextBase` обеспечивают единый паттерн Unit of Work.

---
## 2. Репозитории

### Доступные базовые классы

| Класс                                | Интерфейсы                                             | Особенности                                                               |
| ------------------------------------ | ------------------------------------------------------ | ------------------------------------------------------------------------- |
| `GenericReadOnlyRepository<TEntity>` | `IReadOnlyRepository<TEntity>`                         | Может работать с любыми сущностями                                        |
| `GenericRepository<TEntity>`         | `IRepository<TEntity>`, `IReadOnlyRepository<TEntity>` | Для агрегатов (`IAggregateRoot`), может использоваться как read-only      |
| `GenericRepository<TEntity, TId>`    | `IRepository<TEntity, TId>`                            | Для агрегатов с идентификатором, поддерживает `AddOrUpdate` и поиск по Id |

### Пример создания собственного репозитория

```csharp
public class OrderRepository : GenericRepository<OrderEntity, int>
{
    public OrderRepository(DbContext context)
    {
        SetContext(context);
    }
}

public class ProductReadOnlyRepository : GenericReadOnlyRepository<ProductEntity>
{
    public ProductReadOnlyRepository(DbContext context)
    {
        SetContext(context);
    }
}
```

---

## 3. Примеры использования

### Include/ThenInclude

```csharp
var order = await orderRepo.FindOrThrow(
    orderId,
    q => q.Include(o => o.Customer)
          .ThenInclude(c => c.Address)
          .Include(o => o.Items)
          .ThenInclude(i => i.Product),
    cancellationToken);
```

### Примечания

* Все изменения агрегатов сохраняются через `DbContextBase.SaveChangesAsync()` в рамках **Unit of Work**.
* Репозитории можно расширять и переопределять методы для специфической логики микросервиса.
* Транзакции выполняются безопасно: если текущей нет, создаётся новая; при ошибке происходит **rollback**

