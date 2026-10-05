# Генерация решения

## Установка шаблона

Шаблон лежит в папке `template` этого репозитория, рядом со своим `.template.config`:

```bash
dotnet new install /path/to/DotNetSolutionKit/template
```

После получения изменений установите его заново поверх старого:

```bash
dotnet new install /path/to/DotNetSolutionKit/template --force
```

## Генерация

Первый запуск генерирует общие проекты `Common`, корневой файл решения и первый сервис.
Для него передайте `-M false`:

```bash
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Orders -M false
```

Каждый следующий сервис генерируется со значением по умолчанию `-M true`: создаётся только папка сервиса,
а `Common` берётся уже существующий:

```bash
dotnet new DotNetSolutionKit -N MyCompany -P MyProduct -S Billing
```

`-M true` не трогает корневой `src/Directory.Packages.props`. В решении, сгенерированном этой версией
шаблона, там уже есть все версии, нужные сервису, с любыми `-H` и `--Messaging`; флаги, которые добавляют
файлы в `Common`, выбираются один раз на решение, см. [параметры](#параметры). В более старом решении
сборка останавливается с `NU1010` и называет пакеты без версии; скопируйте их строки `PackageVersion`
из [`Directory.Packages.props`](../../template/src/Directory.Packages.props) шаблона.

Затем добавьте новые проекты в общий файл решения:

```bash
cd src/services
chmod +x manual-add-projects.sh # on Linux and macOS
./manual-add-projects.sh
```

<a id="parameters"></a>
## Параметры

| Параметр | По умолчанию | Что делает |
|---|---|---|
| `-N`, `--NamespaceRoot` | `MyCompany` | Название организации, корневое пространство имён. Может содержать точки. |
| `-P`, `--ProductName` | `Product` | Название продукта. Может содержать точки. |
| `-S`, `--ServiceNameOrCustom` | `Service` | Название сервиса. Может содержать точки. |
| `-M`, `--Minimal` | `true` | `true` генерирует только папку сервиса, `false` - полный комплект: проекты `Common` и `All.sln`. |
| `--Database` | `postgres` | [СУБД](../architecture/persistence.ru.md#sql-server): `postgres` или `mssql` (SQL Server). Одно значение на решение: передайте его с `-M false` и каждому сервису. |
| `-H`, `--Hangfire` | `true` | [Фоновые задачи](../features/background-jobs.md) на Hangfire. Задаётся для каждого сервиса, в том числе с `-M true`. |
| `--Messaging` | `none` | [Шина сообщений](../features/messaging.md): `outbox` или `direct`. Задаётся для каждого сервиса. |
| `-I`, `--Infisical` | `false` | [Секреты из Infisical](../features/secrets.md). |
| `--Vault` | `false` | [Секреты из HashiCorp Vault](../features/secrets.ru.md#hashicorp-vault). |
| `--DiffApi` | `false` | [Сравнение контракта API](../features/api-diff.md) в pull request. |
| `--GitHubCiCd` | `false` | [CI на GitHub Actions](../features/ci.md): сборка, тесты на настоящих серверах, покрытие по запросу, поиск секретов. С `-M false`: workflow покрывают все сервисы в `All.sln`. |
| `-FF`, `--FeatureFlags` | `false` | [Фича-флаги](../features/feature-flags.md). |
| `--HierarchyRules` | `false` | [Правила доступа по дереву тенантов](../features/hierarchy-rules.md). |
| `--Storage` | `false` | [Объектное хранилище](../features/object-storage.md), совместимое с S3. |
| `-CH`, `--ClickHouse` | `false` | [ClickHouse](../features/clickhouse.md): подключения, проверка схемы, готовность. |
| `--MongoDB` | `false` | [MongoDB](../features/mongodb.md) рядом с основной базой: клиент, база сервиса, готовность. |
| `--TestFramework` | `nunit` | Тестовый фреймворк для тестов сервиса, `nunit` или `xunit` (v3); задаётся для каждого сервиса. Собственные тесты `Common` остаются на NUnit. См. [тестирование](../architecture/testing.md). |
| `--Audit` | `false` | [Журнал аудита](../features/audit.md) изменений сущностей; действует только с `--Messaging outbox`. |
| `--ApiGateway` | `false` | [API-шлюз](../features/api-gateway.md) на YARP вместо сервиса. Только с `-M true`. |
| `--Deploy` | `compose` | [Файлы развёртывания](../operations/deployment.md): `compose`, `k8s` или `none`. Одно значение на решение: с `-M true` передавайте то же самое. |
| `--Agent` | `claude` | [Правила и скиллы для ИИ-агента](working-with-ai-agents.md): `claude`, `opencode` или `none`. С `-M false`. |
| `--HttpPort` | свободный порт | Порт в `launchSettings.json` и на хосте под compose. Без параметра берётся свободный порт из диапазона 5000-5999 на машине, где идёт генерация, поэтому сервисы, сгенерированные один за другим, не получают один и тот же порт. |

`-I`, `--Vault`, `--DiffApi`, `--FeatureFlags`, `--HierarchyRules`, `--Storage`, `--ClickHouse`, `--MongoDB` и `--Audit` добавляют
файлы в `Common`, поэтому их нужно передать с `-M false`, когда генерируется `Common`. Каждому сервису,
сгенерированному позже, которому они нужны, передайте `-I`, `--Vault`, `--DiffApi`, `--FeatureFlags`, `--Storage`,
`--ClickHouse`, `--MongoDB` и `--Audit` ещё раз: они меняют и код сервиса, и сервис подключает то, что уже есть в
`Common`. Сервис, сгенерированный без них, обходится без этих частей.

## Имена с точками

Любой из `-N`, `-P` и `-S` может содержать точки:

```bash
dotnet new DotNetSolutionKit -N Acme.Corp -P Shop.Online -S Sales.Orders -M false
```

Эта команда генерирует `Acme.Corp.Shop.Online.Sales.Orders.API` и остальные проекты под тем же именем.
Название сервиса с точкой позволяет разбить домен на небольшие сервисы, например `Sales.Orders` и
`Sales.Invoicing`, вместо одного общего проекта на весь `Sales`. Название продукта с точкой сохраняет
происхождение кода в каждом пространстве имён форка или скачанной копии, где названия репозитория уже нет.

Там, где точка недопустима, имя выводится из названия сервиса:

| Где | Форма | Пример |
|---|---|---|
| Пространства имён, проекты, папки | как задано | `Acme.Corp.Shop.Online.Sales.Orders` |
| Идентификаторы C# | без точек | `SalesOrdersDbContext` |
| Схема базы данных, папка Infisical, имена очередей | в нижнем регистре, точки заменены подчёркиваниями | `sales_orders` |

Сервис называйте по области, а не по его главному агрегату: `-S Basket` с классом `Basket` внутри делает
`Basket` и пространством имён, и типом, и C# не принимает тип там, где видно пространство имён (CS0118).
`Baskets`, `Shopping` или `Sales.Baskets` этого не дают.

<a id="add-the-first-migration"></a>
## Первая миграция

У сгенерированного сервиса есть модель, но нет миграций, поэтому его база данных стартует без таблиц.
Добавьте первую миграцию до первого запуска; базы данных команде не нужно:

```bash
cd src/services/MyCompany.MyProduct.Orders
dotnet ef migrations add Initial \
  -p MyCompany.MyProduct.Orders.Infrastructure \
  -s MyCompany.MyProduct.Orders.Infrastructure \
  -o EntityFramework/Migrations
```

Если сервис сгенерирован с `--Messaging outbox`, миграция создаёт и таблицы outbox. Сервис, запущенный
без миграций, пишет в лог строку уровня fatal с этой командой, а каждый запрос к отсутствующей таблице
падает.

## Настройка и локальный запуск

`appsettings.json` и `appsettings.Local.json` поставляются с пустыми обязательными значениями, и рядом с
каждым лежит ключ `_comment_*` с описанием, что туда вписать. Локальные значения положите в
`appsettings.Secrets.json` в той же папке. Он читается только в окружении `Local`, сгенерированный
`.gitignore` не пускает его в репозиторий, а переменные окружения его переопределяют.

В остальных окружениях передавайте те же ключи переменными окружения, например
`ConnectionStrings__DefaultConnection`.

Профиль запуска `local` задаёт `ASPNETCORE_ENVIRONMENT=Local`, поэтому обычный `dotnet run` читает файл
секретов:

```bash
dotnet run --project src/services/MyCompany.MyProduct.Orders/MyCompany.MyProduct.Orders.API
```

Неправильно настроенный сервис останавливается при старте, а не на первом запросе; см.
[проверки при старте](../operations/startup-checks.md).

Чтобы запустить сервис раньше, чем появятся его база данных, брокер или хранилище секретов, отключите их в
конфигурации, например `Database__Enabled=false`; см. [отключение зависимостей](../operations/switching-dependencies-off.md).
