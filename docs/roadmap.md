# Roadmap

What is planned, and where the template ties a team to one technology. Nothing here is a promise of a
date.

## Planned

| What | Issue |
|---|---|
| `--ApiGateway`: Swagger of the services through the gateway, and permissions asked from a separate service | [#5](https://github.com/Vovanda/DotNetSolutionKit/issues/5) |
| Cookie-only tokens, with CSRF protection for each deployment shape | [#6](https://github.com/Vovanda/DotNetSolutionKit/issues/6) |
| Feature flags: an on-disk snapshot of the external store, and an `important` marker in `features.json` | [#3](https://github.com/Vovanda/DotNetSolutionKit/issues/3) |
| A choice of test framework for a service's tests | |
| A choice of database | |
| A choice of secret store | |
| .NET 9 and later, in a branch of its own | |

## Reducing lock-in

PostgreSQL and NUnit are the defaults because the author of the template prefers them. Another team may
have its own standard or constraints and its own good reasons, and should not have to fight the template
for them, so the template is to let these be replaced. Each item says where the template depends on the technology now and what making it
replaceable takes.

### Database: PostgreSQL by default

PostgreSQL-specific code lives in the infrastructure: the schema guard, the lock around migrations, the
mapping of a unique-constraint violation, case-insensitive search (`ILIKE`), and Hangfire's storage. A
choice of database means a provider layer with an implementation of each, behind a flag, and integration
tests against every supported database.

### Test framework: NUnit by default

The test contexts depend on NUnit in one place: the in-memory database is named after the current test.
Moving the contexts to a project of their own, independent of the framework, lets a flag choose the
framework of a service's tests; the tests of `Common` stay on NUnit.

### Secret store: Infisical

Infisical is the default: open source, simpler and lighter to host than Vault; the reasons are in
[secrets](features/secrets.md#why-infisical). The design does not depend on it: the configuration provider reads secrets through a port,
`ISecretStore`, and the rules that matter (which folder wins, how a name becomes a configuration key, what
happens when the store is unreachable) are the provider's, not the store's. Another store, such as
HashiCorp Vault or a cloud key vault, is an implementation of the port. The one tie left is the options:
the provider takes `InfisicalOptions`, whose folder and optional-store settings are general but whose
project, environment and host are Infisical's, so they would be split.

### CI: GitHub

The only CI the template will generate is for GitHub, behind a flag; other systems are left to the team.
The checks themselves are scripts (`scripts/generate-api-schemas.sh`), so another CI can call them.
