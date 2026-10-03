# Versions of the template

The template is versioned with git tags, by semantic versioning.

| Tag | What it is |
|---|---|
| `v1.0.0` | The template before the breaking changes of 2026. |
| `v2.0.0-preview.1` | The breaking changes in progress. |
| `v2.0.0` | The breaking changes completed and checked. |
| `v2.1.0` | Deployment files, an API gateway, object storage, ClickHouse, CI, end-to-end correlation. Compatible with `v2.0.0`. |

What each version brings is in [version.json](../../version.json) and on the
[releases page](https://github.com/sawking-tech/DotNetSolutionKit/releases).

From `v2.0.0` on, a change keeps generated solutions working, or comes with a short way to update them.
A change that cannot do either is a new major version.

## From v2.0 to v2.1

A service generated from `v2.0.0` builds unchanged on the `Common` of `v2.1.0`, and the `Common` tests
pass: `Common` only gained types and members. A service generated from `v2.1.0` needs that `Common`, so update
`Common` first:

1. Generate a solution from `v2.1.0` into an empty folder with `-M false`, your `-N` and `-P`, your
   flags and any you are adding: `--Storage` and `--ClickHouse` add files to `Common`.
2. Replace your `src/common` and `src/Directory.Packages.props` with the generated ones. Where you changed
   `Common` yourself, merge instead: a diff of the two folders shows your changes next to the template's.
3. Take from it what you want of the new files: `deploy/`, `.github/`, `tools/`. Nothing in your services
   has to change.

Services generated after that take `-M true` and the same flags.

## Do not regenerate over an older solution

`dotnet new ... --force` over a solution generated from `v1` overwrites files that `v2` changed and leaves
the rest, and the result does not build. Generate new solutions from `v2`, and port changes into existing
ones by hand, using the list below.

## What changed in v2

Build and projects:

- Package versions are declared once, in `src/Directory.Packages.props`; a `.csproj` references a package
  by name only.
- A project sees only the projects it references itself: transitive project references are off. The API
  project reaches the domain through the application project.
- `Common.Contracts` depends on `Common`, not the other way round; the user and execution context moved
  into `Common` (`Domain/Context`).
- The version is set by hand in `version.json` instead of Nerdbank.GitVersioning; see
  [ADR-004](../adr/004-product-version-by-hand.md).
- One `Dockerfile` at the solution root builds every service.

Service host:

- The web layer comes from `Common.Web`: `AddPlatformLogging`, `AddPlatformWebApi` and
  `UsePlatformPipeline` replace the service's own `Setup` files for logging, JSON, CORS, Swagger,
  validation and the pipeline. See [web layer](../architecture/web-layer.md).
- The application is built in `SchemaHost.Build`, which `Program` calls.
- The DI container is validated at startup in every environment.
- The service's `DbContext` is registered with `AddDbContext`, not from a pool.

API contract:

- Errors are RFC 9457 problems; the `ErrorResponse` envelope is gone. See [errors](../architecture/errors.md).
- Enums are written as names instead of numbers, and `DateTimeOffset` values in UTC with `Z` instead of
  the server's offset. See [JSON](../architecture/web-layer.md#json).
- A route without an `api/v{n}` segment is listed only in the `all` Swagger document; before, it was
  counted as `v1`. See [Swagger documents](../architecture/web-layer.md#swagger-documents).
- Failed validation answers 422, not 400.
- Health endpoints are `/health` and `/ready` instead of `/healthz` and `/readyz`.

Code:

- `TenantId` replaces `PartnerId` in the user context.
- Domain events dispatch the cascade raised by pre-save handlers, run post-commit and rollback handlers in
  a fresh scope, and reach Hangfire jobs. See [domain events](../architecture/domain-events.md).
- The permission check and `IPermissionService` moved to `Common`; permissions come from the token's
  claims by default. See [authentication and permissions](../architecture/authentication-and-permissions.md).
