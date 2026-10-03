# Testing

Which kind of test a piece of code gets, and why most of them run on an in-memory database:
[ADR-005](../adr/005-testing-a-service.md). This page is how to start.

The service's test project references `Common.Tests`, which brings the test contexts and stubs for time
and the user.

## A service test

A base class builds the context for one class under test: the class, its real repositories and the
`DbContext` on the in-memory provider, with mocks only for what leaves the service.

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

One fixture per method, in `OrderService.Place.Tests.cs`:

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

Each test gets its own database, so fixtures run in parallel. `ActAsync` and `AssertAsync` run in separate
scopes, so the assertion reads what was saved.

## Choosing a context

| Context | For |
|---|---|
| `TestExecutionContext` | a container alone |
| `ServiceTestExecutionContext<TService>` | a class without a database |
| `DbTestExecutionContext<TDbContext>` | a database without a class under test |
| `ServiceDbTestExecutionContext<TService, TDbContext>` | a class and a database on a provider you configure |
| `InMemoryTestExecutionContext<TService, TDbContext>` | a class and the in-memory database, with domain events wired |

## NUnit and Shouldly

Tests use NUnit and Shouldly. Shouldly is MIT-licensed; FluentAssertions moved to a commercial licence
with version 8, which does not fit a template under MIT. NUnit is a preference, but the test contexts
rely on it: the in-memory database is named after `TestContext.CurrentContext.Test.ID`, and per-test
parallelism comes from `[Parallelizable(ParallelScope.All)]`. Moving to xUnit would mean naming the
database another way and accepting its model, where tests in one class run one after another.

## Integration tests

What depends on PostgreSQL (transactions and the outbox, constraints, `ILIKE` and raw SQL, migrations)
runs against a real database in fixtures marked `[Category(TestCategories.Integration)]`. They read the
connection string from `TEST_POSTGRES` and are skipped, with that reason, when it is not set, so a plain
`dotnet test` needs no database:

```bash
docker run -d --name tests-pg -p 15433:5432 -e POSTGRES_PASSWORD=test-do-not-use postgres:16-alpine
TEST_POSTGRES='Host=localhost;Port=15433;Database=postgres;Username=postgres;Password=test-do-not-use' \
  dotnet test --filter "TestCategory=Integration"

dotnet test --filter "TestCategory!=Integration"   # everything else
```

`Common.Tests/Integration` has one such fixture, for numbers from a sequence.
