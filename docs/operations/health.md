# Health

| Endpoint | Answers | Use for |
|---|---|---|
| `/health` | 200 while the process runs; checks no dependency | liveness: restart the container when it stops answering |
| `/ready` | 200 when every check tagged `ready` passes, 503 otherwise | readiness: send traffic only when it answers 200 |

`/health` checks nothing on purpose: a database outage should take the service out of traffic, not get it
restarted in a loop.

Both answer with the same body:

```json
{
  "status": "Healthy",
  "service": "MyCompany.MyProduct.Orders.API",
  "version": "1.2.0",
  "commit": "a1b2c3d",
  "releaseNotes": { "headline": "...", "highlights": [] },
  "timestamp": "2026-10-03T12:00:00+00:00",
  "checks": { "postgres": "Healthy", "hangfire": "Healthy" }
}
```

`version` and `releaseNotes` come from `version.json`, `commit` from the build; see
[ADR-004](../adr/004-product-version-by-hand.md).

## Checks

The checks are standard ASP.NET Core health checks. A generated service has the database (EF Core), and
Hangfire and the message bus when it has them. A new dependency joins `/ready` by registering a check with
the `ready` tag:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<PaymentsGatewayHealthCheck>("payments", tags: [HealthConstants.ReadyTag]);
```

Requests to `/health` and `/ready` are logged at `Verbose`, so probes every few seconds do not fill the
log.
