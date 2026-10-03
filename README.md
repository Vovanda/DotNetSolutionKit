# DotNetSolutionKit

Made by [SawKing Tech](https://sawking.tech/). Author: Vladimir Savkin.

> **Version 2 breaks version 1. Do not apply this template with `--force` over a solution generated from
> v1:** namespaces, project references, package management, versioning and the shape of error responses
> changed, and the overwritten solution will not build. Use v2 for new solutions; port changes into
> existing ones by hand, using [what changed in v2](docs/getting-started/upgrading.md).

A `dotnet new` template for microservices on .NET 8 and PostgreSQL. It generates shared `Common` libraries
and services split into domain, application, infrastructure and API projects, with the host, errors,
validation, persistence, domain events, background jobs and tests already wired, and optional parts behind
flags.

## Install and generate

```bash
dotnet new install /path/to/DotNetSolutionKit/template

# the shared Common projects, the solution file and the first service
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Orders -M false

# every further service
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Billing
```

A release can also be installed from its package: download `SawKing.DotNetSolutionKit.<version>.nupkg` from
the [releases](https://github.com/sawking-tech/DotNetSolutionKit/releases) and run `dotnet new install <file>`.

What the generated solutions look like, with their CI running:
[DotNetSolutionKit.Samples](https://github.com/sawking-tech/DotNetSolutionKit.Samples), one branch per combination
of flags, regenerated from each release.

| Parameter | Default | What it does |
|---|---|---|
| `-N`, `--NamespaceRoot` | `MyCompany` | Organization name, the root namespace. May be dotted. |
| `-P`, `--ProductName` | `Product` | Product name. May be dotted. |
| `-S`, `--ServiceNameOrCustom` | `Service` | Service name. May be dotted. |
| `-M`, `--Minimal` | `true` | `true` generates only the service folder, `false` the full kit. |
| `-H`, `--Hangfire` | `true` | [Background jobs](docs/features/background-jobs.md) on Hangfire. |
| `--Messaging` | `none` | [Message bus](docs/features/messaging.md): `outbox` or `direct`. |
| `-I`, `--Infisical` | `false` | [Secrets and settings from Infisical](docs/features/secrets.md). |
| `--DiffApi` | `false` | [API contract diff](docs/features/api-diff.md) on pull requests. |
| `--GitHubCiCd` | `false` | [CI on GitHub Actions](docs/features/ci.md): build, tests on real servers, coverage, secret scan. |
| `--FeatureFlags` | `false` | [Feature flags](docs/features/feature-flags.md). |
| `--HierarchyRules` | `false` | [Access rules over a tenant tree](docs/features/hierarchy-rules.md). |
| `--Storage` | `false` | [Object storage](docs/features/object-storage.md), S3-compatible. |
| `--ClickHouse` | `false` | [ClickHouse](docs/features/clickhouse.md): connections, schema check, readiness. |
| `--Audit` | `false` | [Audit journal](docs/features/audit.md) of entity changes, through the outbox; with `--Messaging outbox`. |
| `--ApiGateway` | `false` | An [API gateway](docs/features/api-gateway.md) on YARP in place of a service, with `-M true`. |
| `--Deploy` | `compose` | [Deployment files](docs/operations/deployment.md): `compose`, `k8s` or `none`. |
| `--HttpPort` | free port | Port in `launchSettings.json` and on the host under compose. |

Details, dotted names and running locally:
[generating a solution](docs/getting-started/generating-a-solution.md).

## Documentation

Everything else is in [docs](docs/README.md): the architecture, each feature, operations, the decisions
behind the parts that depart from common practice, and the roadmap.

What each version brings: [version.json](version.json) and the
[releases](https://github.com/sawking-tech/DotNetSolutionKit/releases). How versions are made:
[contributing](CONTRIBUTING.md).

## License

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
