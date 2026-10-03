# Persistence

EF Core on PostgreSQL. Each service has its own `DbContext` and its own database schema, named after the
service (`orders`, or `sales_orders` for `Sales.Orders`); Hangfire gets a second schema next to it
(`orders_hangfire`).

## A service owns its schema

Services may share one PostgreSQL database, and nothing in the database stops two services from writing
into one schema. `PostgresSchemaGuard` does, at startup:

1. it creates the schema if it does not exist;
2. it reads the owner recorded in the schema's `_service_metadata` table, under a row lock;
3. it records the service as the owner if there is none, and refuses to start the service if the owner is
   another service.

The owner is the namespace of the service's domain. A copied connection string or a renamed service fails
at startup naming the owner, instead of writing into another service's tables.

When the database is still starting, as when the service and the database are started together, the
guard waits: up to 15 attempts, 2 seconds apart, while PostgreSQL answers "starting up" (SQLSTATE 57P03)
or its port refuses connections.

Why a guard of our own: PostgreSQL has no notion of a schema belonging to an application, and the
alternatives, a database or a database user per service, are infrastructure the template cannot assume.

## Migrations run under a lock

`MigrationRunner.RunMigrations` applies the migrations at startup, holding a PostgreSQL advisory lock
derived from the schema name. Two replicas of one service starting together do not migrate the same
schema twice; the second waits for the first and finds nothing to do.

## Repositories

A repository takes a query as a specification, and filtering, paging, sorting and includes are written
once, in `EntityFrameworkRepository`:

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

Why: [ADR-002](../adr/002-repositories-on-specifications.md).

## Case-insensitive search

`ICaseInsensitiveSearch` builds a search condition as an ordinary specification, so it combines with the
others through `&` and `|`:

```csharp
var query = new QuerySpecification<Order>(
    new OrdersOfCustomer(customerId) & search.GetSpecification<Order>(o => o.Customer.Email, term));
```

The PostgreSQL implementation translates it to `ILIKE` and escapes `%`, `_` and the escape character, so
user input matches literally. Tests on the in-memory database register an in-memory implementation
instead; see [ADR-005](../adr/005-testing-a-service.md).

## Unit of work

`IUnitOfWork` is the transaction boundary of a use case; the service's `DbContext` implements it through
`DbContextBase`. A unique-constraint violation on save becomes a `UniqueViolationException`, which the API
answers with 409.

## Tests

`Common.Tests` covers the base repository, specifications, sorting, `DbContextBase` and the lock key of
the migration runner. The guard, the migrations and the `ILIKE` translation need PostgreSQL and belong to
integration tests.
