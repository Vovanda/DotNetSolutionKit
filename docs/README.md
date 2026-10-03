# Documentation

## Getting started

- [Generating a solution](getting-started/generating-a-solution.md): install, parameters, dotted names,
  running locally
- [Versions of the template](getting-started/upgrading.md): v1 and v2, what changed, how to update

## Architecture

- [Projects and layers](architecture/projects-and-layers.md)
- [Web layer](architecture/web-layer.md): the host and the pipeline every service shares
- [Errors](architecture/errors.md): RFC 9457 problems
- [Validation and pagination](architecture/validation-and-pagination.md)
- [Authentication and permissions](architecture/authentication-and-permissions.md)
- [Persistence](architecture/persistence.md): schemas, migrations, repositories, search
- [Domain events](architecture/domain-events.md)
- [Testing](architecture/testing.md)

## Features

- [Background jobs](features/background-jobs.md): Hangfire, `--Hangfire`
- [Message bus](features/messaging.md): MassTransit, `--Messaging`
- [Secrets from Infisical](features/secrets.md): `-I`
- [Feature flags](features/feature-flags.md): `--FeatureFlags`
- [API diff](features/api-diff.md): `--DiffApi`
- [Access rules over a tenant tree](features/hierarchy-rules.md): `--HierarchyRules`
- [API gateway](features/api-gateway.md): YARP, `--ApiGateway`

## Operations

- [Docker](operations/docker.md): one Dockerfile, only changed services rebuilt
- [Deployment](operations/deployment.md): docker compose or Kubernetes, `--Deploy`
- [Health](operations/health.md): `/health` and `/ready`
- [Startup checks](operations/startup-checks.md)
- [Switching dependencies off](operations/switching-dependencies-off.md): run without the database, the
  jobs, the bus or the secret store, and turn them on later

## Decisions

Where the template departs from a common practice, and why.

- [ADR-001: Domain events run in three phases tied to the transaction](adr/001-three-phase-domain-events.md)
- [ADR-002: Repositories take queries as specifications](adr/002-repositories-on-specifications.md)
- [ADR-003: The API document is read from the built application](adr/003-api-schema-generation.md)
- [ADR-004: The product version is set by hand](adr/004-product-version-by-hand.md)
- [ADR-005: How a service is tested](adr/005-testing-a-service.md)

## [Roadmap](roadmap.md)

What is planned, and where the template ties a team to one technology.
