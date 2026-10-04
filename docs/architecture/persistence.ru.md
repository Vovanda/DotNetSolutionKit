# Хранение

EF Core на PostgreSQL. У каждого сервиса свой `DbContext` и своя схема базы, названная по сервису
(`orders` или `sales_orders` для `Sales.Orders`); рядом Hangfire получает вторую схему (`orders_hangfire`).

## Сервис владеет своей схемой

Сервисы могут делить одну базу PostgreSQL, и сама база не мешает двум сервисам писать в одну схему.
Мешает `PostgresSchemaGuard` при старте:

1. создаёт схему, если её нет;
2. под блокировкой строки читает владельца, записанного в таблице `_service_metadata` схемы;
3. записывает сервис владельцем, если владельца нет, и не даёт сервису стартовать, если владелец - другой
   сервис.

Владелец - namespace домена сервиса. Скопированная строка подключения или переименованный сервис падает
на старте с именем владельца и не пишет в таблицы чужого сервиса.

Если база ещё стартует, например когда сервис и база запущены одновременно, guard ждёт: до 15 попыток с
интервалом 2 секунды, пока PostgreSQL отвечает "starting up" (SQLSTATE 57P03) или его порт отклоняет
подключения.

Почему свой guard: в PostgreSQL нет понятия схемы, принадлежащей приложению, а альтернативы, отдельная
база или отдельный пользователь базы на сервис, - это инфраструктура, на которую шаблон не может
рассчитывать.

## Миграции идут под блокировкой

`MigrationRunner.RunMigrations` применяет миграции при старте, держа advisory lock PostgreSQL, ключ
которого выводится из имени схемы. Две реплики одного сервиса, стартовавшие вместе, не мигрируют схему
дважды: вторая ждёт первую и не находит работы.

Сервис без единой миграции пишет в лог fatal-строку с командой, которая добавляет первую; см.
[генерацию решения](../getting-started/generating-a-solution.ru.md#первая-миграция).

## Репозитории

Репозиторий принимает запрос в виде спецификации, а фильтрация, пагинация, сортировка и include написаны
один раз, в `EntityFrameworkRepository`:

```csharp
public interface IOrderRepository : ISpecificationRepository<Order, Guid>;

public sealed class OrderRepository(OrdersDbContext context)
    : EntityFrameworkRepository<Order, Guid, OrdersDbContext>(context), IOrderRepository
{
    protected override IReadOnlyDictionary<string, string> SortFields { get; } =
        new Dictionary<string, string> { ["placedAt"] = nameof(Order.PlacedAt) };
}

var page = await orders.ListPageAsync(
    new QuerySpecification<Order>(new OrdersOfCustomer(customerId)).Include(o => o.Lines),
    request);
```

Почему так: [ADR-002](../adr/002-repositories-on-specifications.md).

## Поиск без учёта регистра

`ICaseInsensitiveSearch` строит условие поиска как обычную спецификацию, поэтому оно комбинируется с
другими через `&` и `|`:

```csharp
var query = new QuerySpecification<Order>(
    new OrdersOfCustomer(customerId) & search.GetSpecification<Order>(o => o.Customer.Email, term));
```

Реализация для PostgreSQL переводит его в `ILIKE` и экранирует `%`, `_` и сам символ экранирования, так
что ввод пользователя сопоставляется буквально. Тесты на in-memory базе регистрируют вместо неё in-memory
реализацию; см. [ADR-005](../adr/005-testing-a-service.md).

## Читаемые номера

Номер заказа или счёта, который люди читают и набирают, берётся из последовательности PostgreSQL до
создания сущности, поэтому события, которые она поднимает в конструкторе, уже несут номер:

```csharp
var number = await shortIds.GetNextAsync("orders.order_number_seq", ct);
var order = new Order(context, number, ...);
```

`IShortIdGenerator` зарегистрирован в сгенерированном сервисе, последовательность создаёт миграция.
Каждое значение выдаётся один раз, даже если его транзакция откатилась, так что пропуски в нумерации -
норма. Имя последовательности передаётся в запрос параметром и никогда не попадает в его текст.

## Unit of work

`IUnitOfWork` - граница транзакции use case; `DbContext` сервиса реализует его через `DbContextBase`.
Нарушение уникального ограничения при сохранении превращается в `UniqueViolationException`, на которое
API отвечает 409.

## Команды, которые клиент может повторить

Клиент повторяет запрос, когда рвётся соединение или очередь доставляет сообщение заново, и не знает,
прошла ли первая попытка. Команда, которая не должна создать вторую копию, несёт ключ, выбранный
клиентом, а `IIdempotentExecutor` выполняет её работу один раз на ключ:

```csharp
public sealed record CreateOrder(..., string IdempotencyKey) : IIdempotentRequest;

return await idempotent.ExecuteAsync(request, "orders.create", _ =>
{
    var order = new Order(context, ...);
    orders.Add(order);
    return Task.FromResult(order.ToResponse());
}, ct);
```

- Первый запрос выполняет работу и в той же транзакции записывает ключ и ответ; повтор получает этот
  ответ и ничего не делает.
- Два запроса с одним ключом в один момент оба выполняют работу, и уникальный индекс журнала даёт
  закоммитить одному. Второй откатывается, сбрасывает отслеживаемые изменения и отвечает результатом
  победителя.
- Ключ, использованный для другой операции, отклоняется с 409 `IDEMPOTENCY_KEY_REUSED`. Ключ короче 16
  или длиннее 128 символов отклоняется до начала работы.
- Неудачная попытка ничего не записывает, поэтому повтор выполняет работу. Дубликат, который отклоняет
  сама работа, например занятое имя, доходит до вызывающего как этот конфликт: повтор упал бы так же.
- `ExecuteOnceAsync` - для ответа, который нельзя хранить, например секрета, показанного один раз; повтор
  отклоняется с 409 `IDEMPOTENCY_ALREADY_CARRIED_OUT`.
- Ключи ограничены тенантом, от имени которого действует вызывающий, а без тенанта - аккаунтом.

Журнал - таблица в схеме сервиса. По умолчанию её нет; сервис, которому она нужна, добавляет её в
модель и регистрирует executor:

```csharp
// OnModelCreating, then add a migration
modelBuilder.AddIdempotencyLog();

// Infrastructure DependencyInjection
services.AddIdempotency<OrdersDbContext>();
```

## Тесты

`Common.Tests` покрывает базовый репозиторий, спецификации, сортировку, `DbContextBase` и ключ блокировки
migration runner. Guard, миграциям и трансляции в `ILIKE` нужен PostgreSQL, они относятся к
интеграционным тестам. Idempotent executor тестируется на PostgreSQL: повтор, гонка на одном ключе,
повторно использованный и невалидный ключ, неудачная попытка, собственный дубликат работы, два тенанта с
одним ключом.
