# DotNetSolutionKit

[![NuGet](https://img.shields.io/nuget/v/SawKing.DotNetSolutionKit?label=nuget&color=1f9d4c)](https://www.nuget.org/packages/SawKing.DotNetSolutionKit)

Made by Vladimir Savkin at [sawking.tech](https://sawking.tech/).

Site: [dnsk.sawking.tech](https://dnsk.sawking.tech/). Documentation, in English and Russian:
[dnsk.sawking.tech/docs.html](https://dnsk.sawking.tech/docs.html).

> **Version 2 breaks version 1. Do not apply this template with `--force` over a solution generated from
> v1:** namespaces, project references, package management, versioning and the shape of error responses
> changed, and the overwritten solution will not build. Use v2 for new solutions; port changes into
> existing ones by hand, using [what changed in v2](docs/getting-started/upgrading.md).

A `dotnet new` template for microservices on .NET 8 and PostgreSQL or SQL Server. It generates shared `Common` libraries
and services split into domain, application, infrastructure and API projects, with the host, errors,
validation, persistence, domain events, background jobs and tests already wired, and optional parts behind
flags. What it gives a solution in full - the principles, the economics, the cost:
[what the template gives](docs/getting-started/what-it-gives.md).

## Install and generate

```bash
dotnet new install SawKing.DotNetSolutionKit

# the shared Common projects, the solution file and the first service
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Orders -M false

# every further service
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Billing
```

The package is [SawKing.DotNetSolutionKit on nuget.org](https://www.nuget.org/packages/SawKing.DotNetSolutionKit);
every [release](https://github.com/sawking-tech/DotNetSolutionKit/releases) also carries it as a `.nupkg` file.
To try the template from a clone: `dotnet new install /path/to/DotNetSolutionKit/template`.

What the generated solutions look like:
[DotNetSolutionKit.Samples](https://github.com/sawking-tech/DotNetSolutionKit.Samples), a branch each for a
single service, a gateway with services and the full kit, regenerated from every release with their CI
running, and `nightly`, generated from `dev`, where the next release is built. The [samples page](https://dnsk.sawking.tech/samples.html)
shows their files and how their CI ran.

| Parameter | Default | What it does |
|---|---|---|
| `-N`, `--NamespaceRoot` | `MyCompany` | Organization name, the root namespace. May be dotted. |
| `-P`, `--ProductName` | `Product` | Product name. May be dotted. |
| `-S`, `--ServiceNameOrCustom` | `Service` | Service name. May be dotted. |
| `-M`, `--Minimal` | `true` | `true` generates only the service folder, `false` the full kit. |
| `--Database` | `postgres` | [Database](docs/architecture/persistence.md#sql-server): `postgres` or `mssql` (SQL Server). |
| `-H`, `--Hangfire` | `true` | [Background jobs](docs/features/background-jobs.md) on Hangfire. |
| `--Messaging` | `none` | [Message bus](docs/features/messaging.md): `outbox` or `direct`. |
| `-I`, `--Infisical` | `false` | [Secrets and settings from Infisical](docs/features/secrets.md). |
| `--Vault` | `false` | [Secrets and settings from HashiCorp Vault](docs/features/secrets.md#hashicorp-vault). |
| `--DiffApi` | `false` | [API contract diff](docs/features/api-diff.md) on pull requests. |
| `--GitHubCiCd` | `false` | [CI on GitHub Actions](docs/features/ci.md): build, tests on real servers, coverage on demand, secret scan. |
| `-FF`, `--FeatureFlags` | `false` | [Feature flags](docs/features/feature-flags.md). |
| `--HierarchyRules` | `false` | [Access rules over a tenant tree](docs/features/hierarchy-rules.md). |
| `--Storage` | `false` | [Object storage](docs/features/object-storage.md), S3-compatible. |
| `-CH`, `--ClickHouse` | `false` | [ClickHouse](docs/features/clickhouse.md): connections, schema check, readiness. |
| `--MongoDB` | `false` | [MongoDB](docs/features/mongodb.md) beside the main database: the client, the service's database, readiness. |
| `--Notify` | none | Channels of [notifications](docs/features/notifications.md): `email` - through SMTP or Graph API, with a sandbox outside Production; with `--Messaging` other services ask for it by a bus command. |
| `--TestFramework` | `nunit` | Test framework of the service's tests: `nunit` or `xunit` (v3). |
| `--Audit` | `false` | [Audit journal](docs/features/audit.md) of entity changes, through the outbox; with `--Messaging outbox`. |
| `--ApiGateway` | `false` | An [API gateway](docs/features/api-gateway.md) on YARP in place of a service, with `-M true`. |
| `--Deploy` | `compose` | [Deployment files](docs/operations/deployment.md): `compose`, `k8s` or `none`. |
| `--Agent` | `claude` | [Rules and skills for an AI agent](docs/getting-started/working-with-ai-agents.md): `claude`, `opencode` or `none`. |
| `--HttpPort` | free port | Port in `launchSettings.json` and on the host under compose. |

Details, dotted names and running locally:
[generating a solution](docs/getting-started/generating-a-solution.md).

## Documentation

Everything else is in [docs](docs/README.md): the architecture, each feature, operations, the decisions
behind the parts that depart from common practice, and the roadmap. The
[site](https://dnsk.sawking.tech/docs.html) shows the same documents with a menu and rendered diagrams, in
English and Russian.

What each version brings: [version.json](version.json) and the
[releases](https://github.com/sawking-tech/DotNetSolutionKit/releases). How versions are made:
[contributing](CONTRIBUTING.md).

## License

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
