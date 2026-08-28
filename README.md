# DotNetSolutionKit

A high-performance solution template for building microservices on .NET 8+.

This toolkit is engineered to accelerate development by providing a production-ready foundation based on Clean Architecture, DDD, and Zero-Trust Security principles.

## 1. Installation

Navigate to the folder containing .template.config and run:

```bash
dotnet new install .
```

Or specify the absolute path to the template folder:

```bash
dotnet new install /path/to/DotNetSolutionKit
```

To update the template:

```bash
dotnet new uninstall /path/to/DotNetSolutionKit
dotnet new install /path/to/DotNetSolutionKit
```

## 2. Usage

**Create a Full Solution (Infrastructure + First Service)**

Use the `-M false` flag for the initial setup to generate shared Common projects and the root solution file.

```bash
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S IAM -M false
```

**Add Additional Microservices**

For all subsequent services, use the default settings (Minimal mode):

```bash
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Billing
```

**Parameters:**

- `-N` (NamespaceRoot) — Organization name (root namespace).
- `-P` (ProductName) — Product or ecosystem name.
- `-S` (ServiceNameOrCustom) — Specific service name (supports Domain.Service format).
- `-M` (Minimal) — `false` to generate the full kit (Common projects + All.sln); `true` (default) to generate only the service folder.

## 3. Core Features

### 🛡️ Security & IAM

**Dual Authentication:** Out-of-the-box support for JWT Bearer and API Key (X-API-Key) pipelines.

### 🏗️ Architecture

- **Clean Architecture:** Strict separation into API, Application, Infrastructure, and Domain layers.
- **Domain Purity:** Shared logic isolated in Common.Domain to ensure zero infrastructure leakage.

### 🛠️ Developer Experience (DX)

- **Smart Swagger:** Dynamic API version discovery, persistent authorization, and XML documentation support.
- **Advanced Testing:** TestExecutionContext for isolated integration testing with hybrid DI support.
- **Automated Versioning:** Built-in Nerdbank.GitVersioning for git-height-based semantic versions.
- **Global Error Handling:** Centralized IExceptionHandler for 100% consistent error reporting.

### 🚩 Feature Flags

Flags are **data, not code**. They live in one `features.json` that ships from `Common` and is read by
every service, so a feature means the same thing platform-wide, and adding one the UI merely reacts to
costs no deployment.

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

`enabled` is the default; `environments` overrides it per stand, which is what lets one shared file
serve all of them. `effect` says whether turning it on enables or withholds the feature — write flags
so that on means it works, and let a kill switch explain itself in its description. `owner`,
`expiresAt` and `ticket` exist because flags accumulate: an expired flag is reported as expired, so
debt is visible rather than remembered.

Values are layered — the file, then any external store, then environment variables — and the file is
the fallback when a store is unreachable. Each state reports **which layer decided it**, so an
operator toggling a flag that an environment variable overrides can see why nothing changed.

Registration is one call, and nothing to declare:

```csharp
services.AddPlatformFeatureManagement(configuration);
```

After it, three things are available:

| Use | What |
|---|---|
| Evaluate in code | `IFeatureManager` from `Microsoft.FeatureManagement`, backed by the shared file |
| Guard an endpoint | `[FeatureGate(FeatureKeys.SomeFeature)]` — the route is absent while the flag is off |
| Read the platform view | `IFeatureCatalog` — values with owner, expiry, tags and value source |
| Change a value | `IFeatureStore`, or `PUT /api/v1/features/{key}` |

`GET /api/v1/features` returns the whole list and is anonymous — nothing in it is secret, and a client
needs it before anyone signs in. **Enforcement still belongs on the server**: a client list decides
what is drawn, never what is permitted, because a stale mobile cache is not a security boundary.

Two attributes matter beyond evaluation:

- `[BehindFeature("key")]` marks code that exists *because* of a flag — the class or method that goes
  when the flag goes. Retiring a flag always ends in "what can be deleted", and this turns that into a
  search instead of an archaeology exercise.
- `[WithFeature("key")]` lets a test state the state it runs under instead of arranging configuration.

Two tests keep it honest: every constant in `FeatureKeys` names a flag that exists, and keys stay in
the agreed shape.

The full design — schema, layering, the endpoints, retiring a flag, and how a frontend should
integrate with it — is in [docs/feature-flags.md](docs/feature-flags.md).

### 🚀 DevOps

- **Health Checks:** Advanced diagnostics including Service Identity, Build Version, and Git Commit Hash.
- **Docker Ready:** Multi-stage Dockerfiles optimized for .NET 8 LTS runtime.
- **Configuration Validation:** Fail-fast startup with ValidateOnStart for all infrastructure settings.

## 4. Post-Generation Steps

**Synchronize Solution:**

Register new projects in the global solution file:

```bash
cd src/services
chmod +x manual-add-projects.sh # If on Linux/Mac
./manual-add-projects.sh
```

**Restore Dependencies:**

```bash
dotnet restore
```

**Configure & Run:**

Update connection strings in `appsettings.json` and build your solution.

## 📜 **License**  
[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
