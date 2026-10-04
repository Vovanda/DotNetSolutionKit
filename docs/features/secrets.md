# Secrets from Infisical or Vault

Generated with `-I` for [Infisical](https://infisical.com) or `--Vault` for [HashiCorp Vault](https://www.vaultproject.io),
off by default. Pass the flag with `-M false`, and to each service generated later that reads secrets.
What follows holds for both; [Vault](#hashicorp-vault) has its own section for what differs.

The service reads two folders of an Infisical project, the shared one and its own,
and lays them over the rest of configuration. Code reads a secret like any other setting: a secret named
`ConnectionStrings__DefaultConnection` is `configuration.GetConnectionString("DefaultConnection")`.

Keep both secrets and ordinary settings in the store. A value there changes without a new build or a
deployment: edit it in Infisical and restart the service.

## Order

1. `appsettings.json`, `appsettings.<Environment>.json`, `appsettings.Secrets.json` in `Local`;
2. environment variables;
3. the shared folder (`/` by default);
4. the service's folder (`/orders`, or `/sales_orders` for `Sales.Orders`).

The store comes last, so a value it holds wins over anything shipped in the image, and the service's
folder wins over the shared one: a service overrides a shared value without the shared folder having to
know which services exist.

## Configuration

```json
"Infisical": {
  "ProjectId": "",
  "EnvironmentSlug": ""
}
```

`ProjectId` and `EnvironmentSlug` (`dev`, `staging`, `prod`) are required outside `Local`. `HostUri`,
`SharedPath`, `ServicePath` and `Optional` have defaults. `SnapshotPath` is empty: no snapshot is kept
until it is set; see below.

The machine identity, `Infisical__ClientId` and `Infisical__ClientSecret`, comes from environment
variables only. A file in the repository holding the key to the store would bring back the problem the
store solves.

## HashiCorp Vault

With `--Vault` the two folders are two key-value secrets of a KV version 2 engine: the shared one,
`shared` by default, and the service's, `orders` or `sales_orders` for `Sales.Orders`. Each key of a
secret is one setting, named as above.

```json
"Vault": {
  "Address": ""
}
```

`Address` is required outside `Local`. `Mount` (`secret`), `SharedPath`, `ServicePath`, `SnapshotPath` and
`Optional` have defaults. The service signs in with a token, `Vault__Token`, or with an AppRole,
`Vault__RoleId` and `Vault__SecretId`, from environment variables only. A path without a secret reads as
empty, as an empty folder does, so a service with no secrets of its own needs none created; a refusal or
an unreachable Vault is handled as below.

## Why not environment variables

Without `-I`, a service takes its secrets from environment variables, and so do the defaults of this
template (`ConnectionStrings__DefaultConnection`, `RabbitMq__Password`). They are simple, but an
environment variable is readable by anyone with access to the host or the container: `docker inspect`
prints it, `/proc/<pid>/environ` holds it, a process dump contains it, and a CI step that prints the
environment publishes it in its log. A secret in an environment variable is as safe as the least
protected way into that machine.

With `-I`, the secrets live in the store and reach the process only in memory. One secret still has to
come from outside: the machine identity that lets the service read the store. Keep its damage small:

- give the identity read access to one project and one environment only;
- rotate it, and revoke it when a host is retired.

## Why Infisical

The problem it solves: a successful break-in to any one server should not hand over the keys of the
whole system. The internal API key, the key that signs tokens, the broker's credentials sit in the
environment of every server that uses them; with a store, they live in one place with access control, and
the environment of a server holds only the identity that reads its share.

The broker's credentials are the ones most often underrated. Whoever holds them can publish commands and
events that the services carry out as their own, actor headers included, without passing the API, its
permission checks or its request log. A stolen database password is noticed when data leaks; messages
sent through the broker can change things quietly.

The alternatives considered:

- HashiCorp Vault. Mature and widely integrated, but its licence is no longer open source, and it
  brings enterprise complexity for a team that needs a store of secrets and settings.
- OpenBao, the open-source fork of Vault under MPL-2.0. The licence problem is gone; the complexity
  stays.
- Infisical. Open source, simpler, and noticeably lighter to run than Vault, for a team that hosts it
  itself.

The template reads the store through a port, so another store can be added later; see the
[roadmap](../roadmap.md).

## When the store is unreachable

In `Local` the store is optional: a developer runs from `appsettings.Secrets.json` without it. Everywhere
else the service refuses to start without its secrets. A service that starts with a missing connection
string or key fails later, on a request, and further from the cause.

The store is read once, at startup. A secret changed in Infisical applies after the service restarts.

### A snapshot for an outage

With `Infisical__SnapshotPath` set, every successful read is copied to that file, and a start that cannot
reach the store reads the copy instead of refusing:

| The store | A snapshot | The service |
|---|---|---|
| answers | - | starts on the store's values, and replaces the snapshot |
| does not answer | exists | starts on the snapshot, and logs a warning with the time it was written |
| does not answer | none | refuses to start, as without a snapshot; outside `Local` |

The snapshot holds the secrets themselves, which is why it is off until someone responsible for the
service turns it on. It is written readable by its owner alone, and belongs on a volume only the service
mounts. On a container's own filesystem it disappears with the container and protects nothing. A
snapshot that cannot be written does not stop the service; the start logs why, so a missing snapshot is
found before the outage that needs it.

To change one feature flag while the service runs on its snapshot, [pin it](feature-flags.md#pinning-a-flag)
in `features.json` instead of editing the snapshot.

## Tests

`Common.Tests` covers the order of the folders, the optional and required store, and the snapshot: kept
after a read, read when the store does not answer, owner-only on Linux, and a write that fails reported,
with the store replaced by a stub. With `--Vault`, the same rules are checked through the `Vault`
section, and the Vault store itself against a real Vault in development mode (`TEST_VAULT`) in the
integration tests of the template's CI and of a generated solution's.
