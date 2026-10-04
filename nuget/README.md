# DotNetSolutionKit

A `dotnet new` template for microservices on .NET 8 and PostgreSQL or SQL Server. It generates a solution with shared
`Common` libraries and services split into domain, application, infrastructure and API projects. The host,
errors, validation, persistence, domain events, background jobs and tests are wired in every service; the
message bus, secrets, feature flags, object storage, ClickHouse, an API gateway, deployment files and CI are
switched on by flags.

**Site and documentation, in English and Russian: [dnsk.sawking.tech](https://dnsk.sawking.tech/)**

## Install

```bash
dotnet new install SawKing.DotNetSolutionKit
```

## Generate

```bash
# the shared Common projects, the solution file and the first service
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Orders -M false

# every further service
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Billing
```

Generated solutions, regenerated from every release with their CI running:
[DotNetSolutionKit.Samples](https://dnsk.sawking.tech/samples.html).

## Parameters

| Parameter | Default | What it does |
|---|---|---|
| `-N`, `--NamespaceRoot` | `MyCompany` | Organization name, the root namespace. May be dotted. |
| `-P`, `--ProductName` | `Product` | Product name. May be dotted. |
| `-S`, `--ServiceNameOrCustom` | `Service` | Service name. May be dotted. |
| `-M`, `--Minimal` | `true` | `true` generates only the service folder, `false` the full kit. |
| `--Database` | `postgres` | [Database](https://dnsk.sawking.tech/docs.html#persistence:sql-server): `postgres` or `mssql` (SQL Server). |
| `-H`, `--Hangfire` | `true` | [Background jobs](https://dnsk.sawking.tech/docs.html#jobs) on Hangfire. |
| `--Messaging` | `none` | [Message bus](https://dnsk.sawking.tech/docs.html#messaging): `outbox` or `direct`. |
| `-I`, `--Infisical` | `false` | [Secrets and settings from Infisical](https://dnsk.sawking.tech/docs.html#secrets). |
| `--Vault` | `false` | [Secrets and settings from HashiCorp Vault](https://dnsk.sawking.tech/docs.html#secrets:hashicorp-vault). |
| `--DiffApi` | `false` | [API contract diff](https://dnsk.sawking.tech/docs.html#api-diff) on pull requests. |
| `--GitHubCiCd` | `false` | [CI on GitHub Actions](https://dnsk.sawking.tech/docs.html#ci): build, tests on real servers, coverage, secret scan. |
| `--FeatureFlags` | `false` | [Feature flags](https://dnsk.sawking.tech/docs.html#feature-flags). |
| `--HierarchyRules` | `false` | [Access rules over a tenant tree](https://dnsk.sawking.tech/docs.html#hierarchy-rules). |
| `--Storage` | `false` | [Object storage](https://dnsk.sawking.tech/docs.html#storage), S3-compatible. |
| `--ClickHouse` | `false` | [ClickHouse](https://dnsk.sawking.tech/docs.html#clickhouse): connections, schema check, readiness. |
| `--TestFramework` | `nunit` | Test framework of the service's tests: `nunit` or `xunit` (v3). |
| `--Audit` | `false` | [Audit journal](https://dnsk.sawking.tech/docs.html#audit) of entity changes, through the outbox; with `--Messaging outbox`. |
| `--ApiGateway` | `false` | An [API gateway](https://dnsk.sawking.tech/docs.html#gateway) on YARP in place of a service, with `-M true`. |
| `--Deploy` | `compose` | [Deployment files](https://dnsk.sawking.tech/docs.html#deployment): `compose`, `k8s` or `none`. |
| `--Agent` | `claude` | [Rules and skills for an AI agent](https://dnsk.sawking.tech/docs.html#agents): `claude`, `opencode` or `none`. |
| `--HttpPort` | free port | Port in `launchSettings.json` and on the host under compose. |

Dotted names, running locally and the rest of the details:
[generating a solution](https://dnsk.sawking.tech/docs.html#generating).

## Versions

Version 2 targets .NET 8. To install a given version:

```bash
dotnet new install SawKing.DotNetSolutionKit::2.6.2
```

Do not apply version 2 with `--force` over a solution generated from version 1: namespaces, project references,
package management and the shape of error responses changed. What changed and how to carry it over:
[versions of the template](https://dnsk.sawking.tech/docs.html#upgrading). What each release brings:
[releases](https://github.com/sawking-tech/DotNetSolutionKit/releases).

## Source and license

[github.com/sawking-tech/DotNetSolutionKit](https://github.com/sawking-tech/DotNetSolutionKit), MIT.
Made by Vladimir Savkin at [sawking.tech](https://sawking.tech/).
