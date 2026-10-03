/* ==== THE SITE OF THIS REPOSITORY ==============================================================
   Everything the engine (site.css, docs.css, boot.js, panel.js, sections.js, docs.js) needs to know
   about the project it stands in. The engine files are the same in every repository; this one is
   the project's own, and so are index.html and docs.html. */
window.SITE = {
  // the product, in the title of a document page
  name: "DotNetSolutionKit",
  // the prefix of what the browser remembers: theme, language, the side of the panel
  key: "dnsk",
  // where a document's source lives; a document's path is appended to it
  repo: "https://github.com/sawking-tech/DotNetSolutionKit/blob/master/",
  // the document shown when the address names none, and old anchors still out in the world
  defaultDoc: "generating",
  aliases: {},
  /* The documents, by the path they have in the repository without the extension. The last field
     says whether a Russian twin (<path>.ru.md) exists: switch it on when the translation is added,
     and the page shows it to a reader of Russian. */
  shelf: [
    { group: ["Getting started", "С чего начать"], docs: [
      ["generating", "docs/getting-started/generating-a-solution", ["Generating a solution", "Генерация решения"], true],
      ["upgrading", "docs/getting-started/upgrading", ["Versions of the template", "Версии шаблона"], true],
      ["overview", "docs/README", ["The documentation at a glance", "Обзор документации"], true],
      ["roadmap", "docs/roadmap", ["Roadmap", "Дорожная карта"], true],
    ]},
    { group: ["Architecture", "Архитектура"], docs: [
      ["layers", "docs/architecture/projects-and-layers", ["Projects and layers", "Проекты и слои"], true],
      ["web-layer", "docs/architecture/web-layer", ["Web layer", "Веб-слой"], true],
      ["errors", "docs/architecture/errors", ["Errors", "Ошибки"], true],
      ["validation", "docs/architecture/validation-and-pagination", ["Validation and pagination", "Валидация и пагинация"], true],
      ["auth", "docs/architecture/authentication-and-permissions", ["Authentication and permissions", "Аутентификация и права"], true],
      ["persistence", "docs/architecture/persistence", ["Persistence", "Хранение"], true],
      ["domain-events", "docs/architecture/domain-events", ["Domain events", "Доменные события"], true],
      ["testing", "docs/architecture/testing", ["Testing", "Тестирование"], true],
    ]},
    { group: ["Features", "Возможности"], docs: [
      ["jobs", "docs/features/background-jobs", ["Background jobs", "Фоновые задачи"], false],
      ["messaging", "docs/features/messaging", ["Message bus", "Шина сообщений"], false],
      ["secrets", "docs/features/secrets", ["Secrets from Infisical", "Секреты из Infisical"], false],
      ["feature-flags", "docs/features/feature-flags", ["Feature flags", "Фича-флаги"], false],
      ["api-diff", "docs/features/api-diff", ["API diff", "Дифф API"], false],
      ["ci", "docs/features/ci", ["CI on GitHub Actions", "CI на GitHub Actions"], false],
      ["hierarchy-rules", "docs/features/hierarchy-rules", ["Access rules over a tenant tree", "Правила доступа по дереву тенантов"], false],
      ["storage", "docs/features/object-storage", ["Object storage", "Объектное хранилище"], false],
      ["clickhouse", "docs/features/clickhouse", ["ClickHouse", "ClickHouse"], false],
      ["audit", "docs/features/audit", ["Audit journal", "Журнал аудита"], false],
      ["gateway", "docs/features/api-gateway", ["API gateway", "API-шлюз"], false],
    ]},
    { group: ["Operations", "Эксплуатация"], docs: [
      ["docker", "docs/operations/docker", ["Docker", "Docker"], false],
      ["deployment", "docs/operations/deployment", ["Deployment", "Развёртывание"], false],
      ["health", "docs/operations/health", ["Health", "Здоровье"], false],
      ["startup-checks", "docs/operations/startup-checks", ["Startup checks", "Проверки при старте"], false],
      ["switching-off", "docs/operations/switching-dependencies-off", ["Switching dependencies off", "Отключение зависимостей"], false],
    ]},
    { group: ["Decisions", "Решения"], docs: [
      ["adr-001", "docs/adr/001-three-phase-domain-events", ["ADR-001: Domain events in three phases", "ADR-001: доменные события в три фазы"], false],
      ["adr-002", "docs/adr/002-repositories-on-specifications", ["ADR-002: Repositories on specifications", "ADR-002: репозитории на спецификациях"], false],
      ["adr-003", "docs/adr/003-api-schema-generation", ["ADR-003: The API document from the built application", "ADR-003: документ API из собранного приложения"], false],
      ["adr-004", "docs/adr/004-product-version-by-hand", ["ADR-004: The product version by hand", "ADR-004: версия продукта вручную"], false],
      ["adr-005", "docs/adr/005-testing-a-service", ["ADR-005: How a service is tested", "ADR-005: как тестируется сервис"], false],
    ]},
  ],
};
