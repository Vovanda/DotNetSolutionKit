# Domain Events System (3-Phase Pipeline)

A robust domain event orchestration engine integrated with the Entity Framework Core transaction lifecycle. It provides a clean separation between synchronous domain logic, reliable messaging, and post-commit background tasks.

### Core Architecture Principles

1. Encapsulation by Design: Technical cleanup methods (ClearDomainEvents) are hidden from the Application Layer using Explicit Interface Implementation (IHasDomainEvents).
2. Resource Efficiency (Opt-in Events): Domain event support is decoupled from the base Entity<TId>. Events are only processed for entities implementing IHasDomainEvents (like AggregateRoot<TId>). This keeps lightweight entities (lookups, join tables) free from event-related memory overhead.
3. DI Resilience: Infrastructure resolution is safe for DbContextPool and background tasks (Data Seeding/Hangfire), avoiding "Scoped service from root provider" errors.

### Core Handlers Interfaces

Depending on the business requirements, implement one or more of these specialized interfaces:

- **IDomainPreSaveHandler<TEvent>**: Executed within the transaction before saving. Used for validations and Outbox publishing.
- **IDomainPostCommitHandler<TEvent>**: Executed after a successful commit. Used for side effects like Hangfire jobs or analytics.
- **IDomainRollbackHandler<TEvent>**: Executed if the transaction fails. Used for compensation logic or detailed error logging.

---

### 3-Phase Architecture

The pipeline automatically dispatches events across three distinct execution windows:

1. **Phase 1: Pre-Save (Transactional Consistency)**
   Executed inside the `IUnitOfWork.SaveChangesAsync()` call, before the transaction is finalized.
- **Usage:** Validations, data enrichment, and **MassTransit Outbox** publishing.
- **Guarantee:** If the DB transaction fails, no messages are sent to the broker. Everything is atomic.

2. **Phase 2: Post-Commit (Success Side-Effects)**
   Executed immediately after `IUnitOfWork.CommitTransactionAsync()` successfully completes.
- **Usage:** **Hangfire** job enqueuing, analytics, or non-critical notifications.
- **Note:** The transaction is already closed. Any further DB changes require a new `SaveChangesAsync` call.

3. **Phase 3: Rollback (Fault Tolerance)**
   Executed if `IUnitOfWork.RollbackTransactionAsync()` is triggered or a DB command fails.
- **Usage:** Logging specific errors, clearing external cache, or compensatory actions.
- **Context:** Handlers receive the `Exception` object that caused the failure via `HandleRollback` method.

---

### Implementation Example

Managing a complex lifecycle (Validation + Outbox + Background Job) in a single handler:
```C#
public class PasswordResetHandler :
IDomainPreSaveHandler<PasswordResetRequestedEvent>,   // Phase 1
IDomainPostCommitHandler<PasswordResetRequestedEvent>, // Phase 2
IDomainRollbackHandler<PasswordResetRequestedEvent>   // Phase 3
{
private readonly IUnitOfWork _uow;
private readonly IPublishEndpoint _publishEndpoint;

    public PasswordResetHandler(IUnitOfWork uow, IPublishEndpoint publishEndpoint) 
    {
        _uow = uow;
        _publishEndpoint = publishEndpoint;
    }

    // Phase 1: Inside Transaction
    public async Task Handle(PasswordResetRequestedEvent @event, CancellationToken ct, object? data)
    {
        // Reliability: Message and DB changes are bound to the same transaction
        await _publishEndpoint.Publish(@event, ct);
    }

    // Phase 2: After Successful Commit
    async Task IDomainEventHandler<PasswordResetRequestedEvent>.Handle(PasswordResetRequestedEvent @event, CancellationToken ct, object? data)
    {
        // Enqueue long-running background task via Hangfire
        BackgroundJob.Enqueue<IEmailService>(x => 
            x.SendPasswordResetEmailAsync(@event.Email, @event.Link, @event.TokenLifetime, CancellationToken.None));
    }

    // Phase 3: On Failure
    public async Task HandleRollback(PasswordResetRequestedEvent @event, Exception? exception, CancellationToken ct)
    {
        // Log detailed error or perform cleanup
    }
}
```
---

### Infrastructure Guarantees

1. **UOW Independence**: The system is built on EF Core Interceptors. It works seamlessly whether you use `IUnitOfWork`, `Repository Pattern`, or direct `DbContext` calls. Any operation triggering `SaveChangesAsync` or DB transaction events will fire the pipeline.
2. **Recursion & Cycle Protection**: An internal `IsDispatching` flag prevents infinite loops. If a handler calls `IUnitOfWork.SaveChangesAsync()`, the pipeline skips re-triggering for that specific scope.
3. **Immediate Event Clearing**: Domain events are cleared from entities strictly before dispatching starts, ensuring no duplicates during nested saves.
4. **Transaction Resilience**: `IDomainEventStorage` is cleared within `finally` blocks, preventing "event leaking" between requests in the same DI Scope.
5. **Unit of Work Sync**: Calling `IUnitOfWork.DiscardChanges()` explicitly clears all pending domain events, keeping memory and database state perfectly synchronized.

---

### Registration & Setup

#### Step 1: Register Infrastructure & Handlers
In your microservice **Program.cs**:
```C#
// Fast way (registers Core, Persistence, and scans assembly for Handlers)
services.AddDomainEvents(typeof(MyAssemblyMarker).Assembly);

// Or Granular way
services.AddDomainEventCore();
services.AddDomainEventPersistence();
services.AddDomainEventHandlers(typeof(MyAssembly).Assembly);
```
#### Step 2: Configure DbContext
Inject all interceptors into your **DbContext** using the fluent helper:

```C#
services.AddDbContext<MyDbContext>((sp, options) =>
{
options.UseNpgsql(connectionString);
options.ApplyDomainEventInterceptors(sp);
});
```