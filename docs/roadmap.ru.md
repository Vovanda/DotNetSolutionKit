# Дорожная карта

Выпущенные релизы, текущий шаг и запланированные шаги по порядку. Ось считает шаги, а не даты: чем длиннее полоса, тем больше шаг. Полосы, стоящие рядом, можно делать параллельно.

```mermaid
gantt
    dateFormat X
    axisFormat %s
    tickInterval 1second

    section Done
    v1.0 the template before 2026                         :done, s1, 0, 3000ms
    v2.0 layered services, shared host, flags             :done, s2, after s1, 3000ms
    v2.1 deployment, API gateway, CI, correlation         :done, s3, after s2, 2000ms
    v2.2 idempotent commands                              :done, s4, after s3, 500ms
    v2.3 surviving a secret store outage                  :done, s5, after s4, 500ms
    v2.4 tokens in cookies, CSRF                          :done, s6, after s5, 500ms
    v2.5 audit journal                                    :done, s7, after s6, 1000ms
    v2.6 NUnit or xUnit                                   :done, s8, after s7, 1000ms

    section Now
    v2.7 site, samples, nuget.org, major check, gateway Swagger and permissions (#5) :active, s9, after s8, 2000ms

    section Next
    Upgrade path between majors                           :s11, after s9, 1000ms
    HashiCorp Vault as a second secret store              :s12, after s9, 1500ms
    Settings and secrets reloaded without a redeploy      :s13, after s12, 1000ms
    SQL Server as a second database                       :s14, after s9, 3000ms
    3.0 on .NET 10 in master, 2.x stays on .NET 8         :s15, after s11 s13 s14, 2500ms
```

Что принёс каждый релиз: [version.json](https://github.com/sawking-tech/DotNetSolutionKit/blob/master/version.json).
Всё из Next входит в 2.x, на .NET 8, до 3.0: поддержка .NET 8 заканчивается 10 ноября 2026 года, и с 3.0 ветка 2.x
получает только исправления.

## Меньше привязки к технологиям

Технологии по умолчанию выбраны по вкусу автора. Команда со своим стандартом должна иметь возможность их заменить.

| Технология | Где шаблон от неё зависит сейчас |
|---|---|
| PostgreSQL | защита схемы, блокировка миграций, разбор нарушения уникальности, поиск через `ILIKE`, хранилище Hangfire |
| Infisical | только настройки: секреты читаются через порт `ISecretStore`, см. [секреты](features/secrets.ru.md#почему-infisical) |
| GitHub CI | только сгенерированный workflow; проверки - это скрипты, которые вызовет любой CI |
