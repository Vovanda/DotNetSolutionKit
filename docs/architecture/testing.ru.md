# Тестирование

Какой вид теста получает код и почему большинство тестов работает на in-memory базе:
[ADR-005](../adr/005-testing-a-service.md). Эта страница - о том, как начать.

Тестовый проект сервиса ссылается на `Common.Testing`: тестовые контексты, тестовые базы PostgreSQL,
заглушки для времени и пользователя, проверки правил. Тестового фреймворка в нём нет; его выбирает
тестовый проект сервиса и задаёт в `TestSkip.Handler`, как этот фреймворк пропускает тест.
`Common.Tests` - тестовый проект самого `Common`.

`--TestFramework xunit` генерирует тесты сервиса на xUnit v3 вместо NUnit; инфраструктура та же.
Интеграционный тест на xUnit несёт категорию как trait,
`[Trait(TestCategories.TraitName, TestCategories.Integration)]`, поэтому фильтры
`TestCategory=Integration` и `TestCategory!=Integration` отбирают его так же, как категорию NUnit.

## Тест сервиса

Базовый класс строит контекст для одного тестируемого класса: сам класс, его настоящие репозитории и
`DbContext` на in-memory провайдере, а моки - только для того, что выходит за пределы сервиса.

```csharp
internal abstract class OrderServiceTestBase
{
    protected InMemoryTestExecutionContext<OrderService, OrdersDbContext> CreateTestContext()
    {
        var context = new InMemoryTestExecutionContext<OrderService, OrdersDbContext>();

        var mail = new Mock<IMailSender>();
        context.Register(mail);                    // the mock, to verify after the act
        context.Register(mail.Object);             // the service the class under test receives

        context.Register<IOrderRepository, OrderRepository>();
        context.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrdersDbContext>());
        return context;
    }
}
```

Одна fixture на метод, в `OrderService.Place.Tests.cs`:

```csharp
[TestFixture]
[TestOf(typeof(OrderService))]
[Parallelizable(ParallelScope.All)]
internal class OrderServicePlaceTests : OrderServiceTestBase
{
    [Test(Description = "A placed order is saved with its lines")]
    public async Task Should_SaveTheOrder_When_TheCartHasLines()
    {
        await using var ctx = CreateTestContext();
        await ctx.ArrangeAsync(Customer);

        var orderId = await ctx.ActAsync(s => s.PlaceAsync(new PlaceOrder(Customer.Id, Lines), default));

        await ctx.AssertAsync(async db => (await db.Orders.FindAsync(orderId)).ShouldNotBeNull());
    }
}
```

У каждого теста своя база, поэтому fixture'ы идут параллельно. `ActAsync` и `AssertAsync` работают в
разных scope, поэтому проверка читает то, что сохранено.

## Выбор контекста

| Контекст | Для чего |
|---|---|
| `TestExecutionContext` | только контейнер |
| `ServiceTestExecutionContext<TService>` | класс без базы |
| `DbTestExecutionContext<TDbContext>` | база без тестируемого класса |
| `ServiceDbTestExecutionContext<TService, TDbContext>` | класс и база на провайдере, который настраиваете вы |
| `InMemoryTestExecutionContext<TService, TDbContext>` | класс и in-memory база с подключёнными доменными событиями |

## NUnit и Shouldly

Тесты используют NUnit и Shouldly. Shouldly под лицензией MIT; FluentAssertions с версии 8 перешёл на
коммерческую лицензию, которая не подходит шаблону под MIT. NUnit - предпочтение, но тестовые контексты
на него опираются: in-memory база называется по `TestContext.CurrentContext.Test.ID`, а параллельность
на уровне теста даёт `[Parallelizable(ParallelScope.All)]`. Переход на xUnit означал бы другое
именование базы и его модель, где тесты одного класса идут друг за другом.

## Интеграционные тесты

То, что зависит от PostgreSQL (транзакции и outbox, ограничения, `ILIKE` и сырой SQL, миграции),
проверяется на настоящей базе в fixture'ах с пометкой `[Category(TestCategories.Integration)]`. Они
читают строку подключения из `TEST_POSTGRES` и пропускаются с этой причиной, если переменная не задана,
поэтому обычному `dotnet test` база не нужна:

```bash
docker run -d --name tests-pg -p 15433:5432 -e POSTGRES_PASSWORD=test-do-not-use postgres:16-alpine
TEST_POSTGRES='Host=localhost;Port=15433;Database=postgres;Username=postgres;Password=test-do-not-use' \
  dotnet test --filter "TestCategory=Integration"

dotnet test --filter "TestCategory!=Integration"   # everything else
```

Такие fixture'ы есть в `Common.Tests/Integration`: для номеров из последовательности и для объектного
хранилища.

## Покрытие

Тестовые проекты ссылаются на `coverlet.collector`, поэтому покрытие не требует настройки:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Каждый тестовый проект пишет отчёт Cobertura в `TestResults/`. С `--GitHubCiCd` CI сводит отчёты
unit- и интеграционных прогонов и может падать по порогу покрытия ветвей; см. [CI](../features/ci.md).
