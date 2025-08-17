# DotNetSolutionKit

Шаблон решения для микросервисной архитектуры на платформе .NET. 

Включает:
- реализацию микросервиса по принципам чистой архитектуры и DDD
- общие проекты для бизнес-логики и контрактов
- инфраструктуру и вспомогательные инструменты.

---
## Цель

Создать удобный и расширяемый шаблон для быстрого старта новых проектов с готовой структурой и настройками.


## 1. Установка

1. Убедитесь, что у вас установлен **.NET 8 SDK** или выше:

```bash
dotnet --version
```

2. Установите шаблон в глобальные шаблоны `dotnet`:

```bash
dotnet new --install path/to/DotNetSolutionKit
```

> После этого шаблон будет доступен через команду `dotnet new DotNetSolutionKit`.
---

## 2. Структура

* `e2e/` — проекты для end-to-end тестирования
* `docker/`, `.helm/`, `grafana/` — DevOps и инфраструктурные конфигурации
* `src/common/` — проекты с общей бизнес-логикой и контрактами
* `src/services/` — микросервисы (API, Application, Infrastructure, Tests)
* `src/framework/` — проектный framework для конфигурации, логирования и общих сервисов
---

## 3. Параметры шаблона

* `NamespaceRoot` — корень пространства имён, обычно название организации (по умолчанию `MyCompany`)
* `ProductName` — имя продукта (по умолчанию `Product`)
* `ServiceNameOrCustom` — имя сервиса или произвольное значение, например, домен и сервис в формате `Domain.Service` (по умолчанию `Service`)
* `Minimal` — true = только микросервис; false = полный multi-solution kit. По умолчанию `true`.

Эти параметры подставляются в имена файлов, папок и содержимое, формируя namespace и структуру проекта.

---

## 4. Примеры использования

### 4.1 Создание полного решения с APIGateway и набором сервисов

```bash
dotnet new DotNetSolutionKit \
  --NamespaceRoot "Sawking" \
  --ProductName "Avox" \
  --ServiceNameOrCustom "APIGateway" \
  --Minimal false
```

**Результат:**

* Полный multi-solution, включающий общий `.sln` файл (`All.sln`)
* APIGateway сервис в `src/services/APIGateway`
* Все необходимые проекты (`Common`, `Framework`) добавлены и подключены
* Можно добавлять новые микросервисы и подключать их через общий `.sln`

**После добавления новых микросервисов:**

* Используйте скрипт `manual-add-projects.sh` (в `src/services`) для автоматического добавления проектов в `All.sln`.
* Скрипт проверяет, какие проекты ещё не подключены, и добавляет их, пропуская уже существующие.

---

### 4.2 Создание одного микросервиса Knowledge (по умолчанию Minimal = true)

```bash
dotnet new DotNetSolutionKit \
  --NamespaceRoot "Sawking" \
  --ProductName "Avox" \
  --ServiceNameOrCustom "KnowledgeService"
```

**Результат:**

* Папка `src/services/KnowledgeService` с API, Application, Infrastructure и тестами
* Стартовый микросервис без генерации инфрасттруктурных папок/проектов (Common, Framework)
* Можно интегрировать в полный solution позже с помощью скрипта `manual-add-projects.sh`

---

## 5. После генерации

* Выполните `dotnet restore` для восстановления всех зависимостей
* При необходимости проверьте и отредактируйте конфигурационные файлы (например, `appsettings.json`)
