# DotNetSolutionKit

> **BREAKING CHANGES SINCE OCTOBER 2026. DO NOT APPLY THIS TEMPLATE WITH `--force` OVER A SOLUTION
> GENERATED FROM AN EARLIER VERSION.** Namespaces, project references, package management and
> versioning changed, and the overwritten solution will not build. Use the template for new
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
- `-I` (Infisical): read secrets from Infisical. The shared folder and the service's folder overlay
  configuration, so code reads a secret like any other setting. Pass it together with `-M false`, since
  the secret store lives in `Common`; a service generated later with `-I` uses it from there.

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
- Background jobs run on Hangfire, stored in PostgreSQL in a separate schema per service.

### Development

- Swagger builds one document per API version; the versions are discovered from the routes, and XML
  comments become the descriptions.
- `TestExecutionContext` in `Common.Tests` runs integration tests against a real DI container.
- The version is set by hand in `version.json`, next to its release notes, and `Directory.Build.props`
  passes it to every assembly. `/healthz` reports it together with the commit in a separate field:
  the short git SHA recorded at build time, or `GIT_SHA` from the environment where the build had no
  `.git` folder.
- Package versions are declared once, in `src/Directory.Packages.props`; a `.csproj` references a
  package by name only. Every version stays on the .NET 8 line: no package pulls in .NET 9
  libraries, directly or transitively.
- A global `IExceptionHandler` maps exceptions to one error shape. Errors come back in an
  `ErrorResponse` envelope; successful responses return the DTO as is.

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
- `/healthz` and `/readyz` report the service name and build version; `/readyz` also checks the
  database connection.
- Each service has a multi-stage Dockerfile.

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
