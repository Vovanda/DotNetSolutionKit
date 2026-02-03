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
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Auth -M false
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

**Clean Architecture:** Strict separation into API, Application, Infrastructure, and Domain layers.
**Domain Purity:** Shared logic isolated in Common.Domain to ensure zero infrastructure leakage.

### 🛠️ Developer Experience (DX)

**Smart Swagger:** Dynamic API version discovery, persistent authorization, and XML documentation support.
**Advanced Testing:** TestExecutionContext for isolated integration testing with hybrid DI support.
**Automated Versioning:** Built-in Nerdbank.GitVersioning for git-height-based semantic versions.
**Global Error Handling:** Centralized IExceptionHandler for 100% consistent error reporting.

### 🚀 DevOps

**Health Checks:** Advanced diagnostics including Service Identity, Build Version, and Git Commit Hash.
**Docker Ready:** Multi-stage Dockerfiles optimized for .NET 8 LTS runtime.
**Configuration Validation:** Fail-fast startup with ValidateOnStart for all infrastructure settings.

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
