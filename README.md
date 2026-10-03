# DotNetSolutionKit

> **BREAKING CHANGES SINCE OCTOBER 2026. DO NOT APPLY THIS TEMPLATE WITH `--force` OVER A SOLUTION
> GENERATED FROM AN EARLIER VERSION.** Namespaces, project references, package management and
> versioning changed, and the overwritten solution will not build. Error responses changed shape for
> API clients: the `ErrorResponse` envelope is gone, errors are RFC 9457 problems. Use the template for new
> solutions; port changes into existing ones by hand.

A `dotnet new` template for microservices on .NET 8 and PostgreSQL. It generates a set of shared
`Common` libraries and a service split into domain, application, infrastructure and API projects.

## 1. Installation

The template lives in the `template` folder of this repository, next to its `.template.config`:

```bash
dotnet new install /path/to/DotNetSolutionKit/template
```

To update it after pulling changes:

```bash
dotnet new install /path/to/DotNetSolutionKit/template --force
```

## 2. Usage

The first run generates the shared `Common` projects, the root solution file and the first service.
Pass `-M false` for it:

```bash
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S IAM -M false
```

Every further service is generated with the default `-M true`, which creates the service folder only:

```bash
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Billing
```

Parameters:

- `-N` (NamespaceRoot): organization name, the root namespace.
- `-P` (ProductName): product name.
- `-S` (ServiceNameOrCustom): service name; a dotted name such as `Domain.Service` works too.
- `-M` (Minimal): `false` generates the full kit (`Common` projects and `All.sln`), `true` (default)
  only the service folder.
- `--Hangfire false`: no background jobs. By default each service gets Hangfire, stored in PostgreSQL
  in its own schema, with the dashboard; the flag works per service, also with `-M true`.
- `--Messaging none|outbox|direct`: message bus for the service, `none` by default. `outbox` wires
  MassTransit on RabbitMQ with a transactional outbox in the service's database; `direct` sends straight
  to the broker, and a message is lost if the broker is down. See
  [Messaging/README.md](template/src/common/NamespaceRoot.ProductName.Common.Infrastructure/Messaging/README.md).
- `-I` (Infisical): read secrets from Infisical. The shared folder and the service's folder overlay
  configuration, so code reads a secret like any other setting. Pass it together with `-M false`, since
  the secret store lives in `Common`; a service generated later with `-I` uses it from there.
- `--HierarchyRules`: add access rules over a tenant tree stored as materialized paths
  (`IHierarchicalEntity`, `HierarchyRules`): whether a tenant may see another one in its subtree or
  among its direct children. Off by default; it lives in `Common`, so pass it with `-M false`.

## 3. What the template gives you

### Security

- Every service authenticates every request itself. One composite scheme accepts a JWT, from the
  `Authorization` header or the access-token cookie, or an `X-API-Key` for service-to-service calls.
  A service that only validates tokens needs the public key alone; the signing key stays with the
  service that issues tokens.
- Helpers in `Common.Web` set and clear the access and refresh token cookies.
- `[RequiredPermissions(...)]` on an action declares the permissions it needs; a global filter
  checks them.

### Architecture

- Each service is split into domain, application, infrastructure and API projects. Transitive
  project references are switched off, so a project sees only the projects it references itself:
  a service's domain project references `Common` and nothing else.
- `Common` holds the domain base types: `Entity`, `AggregateRoot`, domain events, `IUnitOfWork`.
  `Common.Application`, `Common.Infrastructure`, `Common.Web` and `Common.Contracts` hold what
  services share at the other layers.
- Domain events go through a three-phase pipeline (pre-save, post-commit, rollback) built on EF Core
  interceptors. See [Events/Readme.md](template/src/common/NamespaceRoot.ProductName.Common.Application/Events/Readme.md).
- Persistence is EF Core on PostgreSQL. Each service owns its schema, and a guard refuses to start a
  service on a schema another service has claimed.
- Repositories take queries as specifications and share one implementation of filtering, paging,
  sorting and includes in `EntityFrameworkRepository`. See
  [docs/repositories-on-specifications.md](docs/repositories-on-specifications.md).
- Background jobs run on Hangfire, stored in PostgreSQL in a separate schema per service.

### Development

- Swagger builds one document per API version; the versions are discovered from the routes, and XML
  comments become the descriptions.
- `TestExecutionContext` in `Common.Tests` runs integration tests against a real DI container.
- The version is set by hand in `version.json`, next to its release notes, and `Directory.Build.props`
  passes it to every assembly. `/health` reports it together with the commit in a separate field:
  the short git SHA recorded at build time, or `GIT_SHA` from the environment where the build had no
  `.git` folder.
- Package versions are declared once, in `src/Directory.Packages.props`; a `.csproj` references a
  package by name only. Every version stays on the .NET 8 line: no package pulls in .NET 9
  libraries, directly or transitively.
- Errors are RFC 9457 problems (`application/problem+json`): `status`, `title`, `detail`, a `code`
  for client code to match on, `traceId` and `correlationId`; validation failures add `errors` with
  field names as they appear in the JSON. One shape covers thrown exceptions (mapped in
  `Common.Web/Errors/PlatformExceptionMapper`), model validation and empty 404/405 responses. A
  service adds its own exception rules by registering an `IExceptionMapping`. Successful responses
  return the DTO as is.

### Feature flags

Flags are data. They live in one `features.json` that ships from `Common` and is read by every
service, so a feature means the same thing in all of them, and a flag that only the UI reacts to
needs no deployment.

```json
{
  "Features": {
    "checkout.new-flow": {
      "enabled": false,
      "effect": "enables",
      "environments": { "Development": true },
      "tags": ["checkout"],
      "description": "Serve the rebuilt checkout instead of the original.",
      "owner": "payments",
      "expiresAt": "2026-12-31",
      "ticket": "ABC-123"
    }
  }
}
```

`enabled` is the default, and `environments` overrides it per environment, so one shared file serves
all of them. `effect` says whether turning the flag on enables or withholds the feature. Write flags so
that on means the feature works, and let a kill switch say so in its description. `owner`, `expiresAt`
and `ticket` exist because flags accumulate: a flag past its date is reported as expired, so the debt
shows up in the data instead of in someone's memory.

Values are layered: the file, then an external store if one is configured, then environment
variables. The file is the fallback when the store is unreachable. Each state reports which layer
decided it, so an operator who toggles a flag that an environment variable overrides can see why
nothing changed.

Registration is one call:

```csharp
services.AddPlatformFeatureManagement(configuration);
```

After it:

| Use | What |
|---|---|
| Evaluate in code | `IFeatureManager` from `Microsoft.FeatureManagement`, backed by the shared file |
| Guard an endpoint | `[FeatureGate(FeatureKeys.SomeFeature)]`: the route is absent while the flag is off |
| Read the platform view | `IFeatureCatalog`: values with owner, expiry, tags and value source |
| Change a value | `IFeatureStore`, or `PUT /api/v1/features/{key}` |

`GET /api/v1/features` returns the whole list and needs no authentication: nothing in it is secret,
and a client needs it before anyone signs in. The list decides what a client draws. What a user is
permitted to do is still checked on the server, because a stale mobile cache is not a security
boundary.

Two attributes work alongside evaluation:

- `[BehindFeature("key")]` marks code that exists because of a flag: the class or method that goes
  when the flag goes. Retiring a flag then starts with a search for the attribute.
- `[WithFeature("key")]` sets the flag state a test runs under, without arranging configuration.

Two tests check the catalogue: every constant in `FeatureKeys` names a flag that exists, and every key
follows the naming rule.

The schema, the layering, the endpoints, retiring a flag and frontend integration are described in
[docs/feature-flags.md](docs/feature-flags.md).

### Operations

- A misconfigured service stops at startup instead of failing on the first request. Settings are
  validated with `ValidateOnStart`, a missing connection string throws, the schema guard refuses a
  schema owned by another service, and in the `Local` environment the container validates every
  registration when it is built.
- `/health` answers while the process runs and checks no dependency; `/ready` runs every check
  tagged `ready` and answers 503 when one fails. Both report the service, the version with its
  release notes, the commit and each check by name. The checks are standard ASP.NET Core health
  checks: the database through EF Core, Hangfire and the message bus when the service has them; a
  new dependency joins `/ready` by registering a check with the `ready` tag.
- One `Dockerfile` at the solution root builds every service. Its first stage holds only `Common`
  and the build props, and each service adds only its own folder on top. A change in one service
  rebuilds that service alone: every other image keeps its layers and digest, so a deploy restarts
  only what changed. A change in `Common` rebuilds every service, with `Common` compiled once.

  ```bash
  docker build --provenance=false     --build-arg SERVICE=MyCompany.MyProduct.Billing     --build-arg GIT_SHA=$(git log -1 --format=%h -- src/common src/services/MyCompany.MyProduct.Billing '*.props' version.json)     -t billing .
  ```

  `GIT_SHA` is the last commit that changed the service's inputs, so an unchanged service gets the
  same value and the same image; `/health` reports it as `commit`. `--provenance=false` leaves out
  the build attestation, which carries a timestamp and would give every build a new digest.

## 4. After generation

Add the new projects to the global solution file:

```bash
cd src/services
chmod +x manual-add-projects.sh # on Linux and macOS
./manual-add-projects.sh
```

`appsettings.json` and `appsettings.Local.json` ship with the required values empty and a
`_comment_*` key next to each one saying what goes there. For local runs, put your values into
`appsettings.Secrets.json` in the same folder. It is read only in the `Local` environment, the
generated `.gitignore` keeps it out of the repository, and environment variables override it. In
other environments, pass the same keys as environment variables, for example
`ConnectionStrings__DefaultConnection`.

The `local` launch profile sets `ASPNETCORE_ENVIRONMENT=Local`, so a plain `dotnet run` reads the
secrets file:

```bash
dotnet run --project src/services/MyCompany.MyProduct.IAM/MyCompany.MyProduct.IAM.API
```

## License

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
