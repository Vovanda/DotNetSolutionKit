# Background jobs

On by default; `--Hangfire false` generates a service without them. Jobs run on Hangfire, stored in
PostgreSQL in a schema of the service's own (`orders_hangfire`), next to the service's schema and guarded
the same way.

## Configuration

```json
"HangfireSettings": {
  "DashboardUser": "admin",
  "DashboardPassword": "",
  "WorkerCount": 10
}
```

The password is required and has no default: the service does not start without it. The dashboard is at
`/hangfire`, behind basic authentication with these credentials.

## Write and enqueue a job

A job is a class in the service's infrastructure; it is resolved from the container in a scope of its own:

```csharp
public sealed class ExpireUnpaidOrdersJob(IOrderRepository orders, IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync()
    {
        // ...
        await unitOfWork.SaveChangesAsync();
    }
}

RecurringJob.AddOrUpdate<ExpireUnpaidOrdersJob>("expire-unpaid-orders", job => job.ExecuteAsync(), Cron.Hourly());
BackgroundJob.Enqueue<ExpireUnpaidOrdersJob>(job => job.ExecuteAsync());
```

What the template adds to plain Hangfire:

- **Domain events work in jobs.** `DomainEventJobActivator` gives each job a scope the domain event
  interceptors can see, so events raised by a job are dispatched as in a request.
- **The actor travels with the job.** `JobActorPropagationFilter` records who enqueued a job and restores
  that user while the job runs, so what the job writes is attributed to them. A scheduled run has no such
  user and runs as the system. A job that acts on its own account even when someone enqueued it takes
  its context from `ISystemExecutionContextFactory.Create()`.
- **`/ready` checks Hangfire.** The health check fails when no Hangfire server is running.

## Why Hangfire

<!-- To confirm with the template's author: the reasons below are the usual ones and need his own. -->

- Fire-and-forget, delayed and recurring jobs through one API, with retries and the history of each run
  stored in PostgreSQL.
- A dashboard out of the box: what ran, what failed and why, with a button to retry.
- Jobs are plain classes resolved from DI, so they are tested like any other class.

Quartz.NET schedules well, but brings no dashboard and needs more code for the same job.

## Tests

A job only orchestrates; its tests mock the ports it calls and verify the calls, and the logic it calls is
tested as a service. See [ADR-005](../adr/005-testing-a-service.md). `Common.Tests` covers the job
activator.
