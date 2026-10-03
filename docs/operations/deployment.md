# Deployment

`--Deploy` chooses the deployment files a solution is generated with:

| Value | What is generated |
|---|---|
| `compose` (default) | `deploy/compose/`: the infrastructure, a script that starts it with every service, an example `.env`; and a `deploy/compose.yml` in each service's folder |
| `k8s` | `deploy/k8s/apply.sh`, and a `deploy/k8s.yaml` in each service's folder: a Deployment and a Service |
| `none` | nothing; the [Dockerfile](docker.md) only |

The two are exclusive: a team that runs Kubernetes gets no compose files to maintain, and the other way
round. With `compose` or `k8s`, `deploy/build-images.sh` builds an image per service.

`dotnet new` does not keep the executable bit, so on Linux and macOS run once:
`chmod +x deploy/*.sh deploy/*/*.sh`.

Each service keeps its deployment file in its own folder, so a service added later with `-M true` brings
its file along and nothing shared has to be edited. The scripts pick up every service's file.

## Images

```bash
deploy/build-images.sh                                         # every service, as local/<service>:latest
deploy/build-images.sh Orders                                  # one service
REGISTRY=registry.example.com TAG=1.4.0 PUSH=1 deploy/build-images.sh
```

An image is named `<REGISTRY>/<service>:<TAG>`, the service in lower case with underscores for dots
(`Sales.Orders` is `sales_orders`). `GIT_SHA` is the last commit that touched the service's inputs, so an
unchanged service gets the same image; see [Docker](docker.md).

## docker compose

```bash
cp deploy/compose/.env.example deploy/compose/.env    # fill in the passwords
deploy/build-images.sh
deploy/compose/compose.sh up -d --wait
```

`compose.sh` runs `docker compose` over `deploy/compose/infrastructure.yml` and every
`src/services/*/deploy/compose.yml`, with `deploy/compose/.env`; any `docker compose` command works through
it (`logs -f`, `down`, `ps`).

- PostgreSQL, and RabbitMQ when a service uses the bus, start first; a service starts once they report
  healthy.
- A service is healthy when its `/ready` answers, so `up --wait` returns when everything can serve, and
  whatever depends on a service waits for its database too.
- Each service is published on the host's loopback only, at the port it was generated with; a reverse
  proxy or a gateway in front publishes it further.
- All services share one database, each in its own schema; see [persistence](../architecture/persistence.md).

## Kubernetes

```bash
REGISTRY=registry.example.com TAG=1.4.0 PUSH=1 deploy/build-images.sh
kubectl create secret generic orders \
  --from-literal=ConnectionStrings__DefaultConnection='Host=...;Database=...;Username=...;Password=...'
REGISTRY=registry.example.com TAG=1.4.0 deploy/k8s/apply.sh
```

`apply.sh` fills the registry and the tag into every `src/services/*/deploy/k8s.yaml` and applies it to the
current `kubectl` context; further arguments go to `kubectl apply` (`--namespace shop`).

- The Deployment runs two replicas and updates them one at a time: a new pod takes traffic once
  `/ready` answers, and an old one leaves only then (`maxUnavailable: 0`).
- Readiness is `/ready`, liveness is `/health`: a database outage takes a pod out of traffic without
  restarting it.
- Settings come from a Secret named after the service (`sales-orders` for `Sales.Orders`) as environment
  variables, the same keys as in `appsettings.json` with `__` for `:`. The Secret is optional, so a pod
  without it starts and reports what is missing.
- The database, the broker and the secret store are not in the manifests: a cluster usually has them
  managed, and their addresses go into the Secret.
- Replicas start together safely: migrations run under a lock, and so does the
  [schema guard](startup-checks.md).

## Checked

Both were run end to end on a generated solution: compose with two services, one generated later with
`-M true`, each answering `/ready` with its own commit; Kubernetes on k3s with four replicas of a service
with a dotted name on a fresh database, all ready without a restart.
