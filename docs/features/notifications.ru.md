# Уведомления

`--Notify` даёт сервису уведомления и выбирает их каналы; `--Notify email` - почта, SignalR
[в планах](https://github.com/sawking-tech/DotNetSolutionKit/issues/71) вторым каналом. Флаг передаётся
сервису, который владеет уведомлениями. Сообщение уходит получателю как есть: тема и простой текст. Письма
из шаблонов придут с `--Templates`, который [в планах](https://github.com/sawking-tech/DotNetSolutionKit/issues/74)
поверх `--Documents`.

| Часть | |
|---|---|
| `INotificationEmailSender` | порт в `Common.Application`: `SendEmailAsync` и `SendEmailWithAttachmentAsync` |
| транспорты | SMTP на MailKit и Microsoft Graph, внутренние; тот, что назван в `Email:Provider`, выбирается один раз при регистрации |
| песочница | вне Production каждое письмо уходит на один адрес с пометкой сверху: кому оно было, окружение и время; ни один транспорт её не обходит |

```csharp
public sealed class InvoiceMailer(INotificationEmailSender email) : IInvoiceMailer
{
    public Task SendAsync(Invoice invoice, byte[] pdf, CancellationToken ct) =>
        email.SendEmailWithAttachmentAsync(invoice.CustomerEmail, $"Invoice {invoice.Number}",
            "Your invoice is attached.", pdf, $"{invoice.Number}.pdf", ct);
}
```

Graph принимает вложение в том же запросе, что и письмо. Microsoft описывает этот способ для файлов меньше
3 МБ; файлу больше нужна
[сессия загрузки](https://learn.microsoft.com/en-us/graph/outlook-large-attachments), которую транспорт не
открывает; для вложения больше нужен SMTP.

## Песочница

Тестовый стенд не должен писать никому снаружи. Каждый транспорт наследует одну базу, чьи методы применяют
песочницу и только потом передают письмо провайдеру: вне Production или пока `Email:Sandbox:Enabled` равно
`true`, письмо уходит на `Email:Sandbox:ReceiverAddress` с пометкой сверху (`[SANDBOX INTERCEPTED]`, исходный
получатель, окружение, время по UTC). Production пишет настоящим получателям, только когда песочница
выключена.

## Настройки

Проверяются на старте: провайдер должен быть назван, в его секции должно быть всё нужное, а адреса (`From`,
`SenderEmail`, `Sandbox:ReceiverAddress`) должны быть адресами почты. Вход на SMTP-сервер задаётся парой
`Username` и `Password` или не задаётся вовсе: с половиной пары транспорт пропустил бы вход, и каждая
отправка падала бы.

| Настройка | |
|---|---|
| `Email:Provider` | `Smtp` или `GraphApi` |
| `Email:TimeoutSeconds` | 30: SMTP, на каждую сетевую операцию доставки; Graph его не читает |
| `Email:Smtp:Host`, `Email:Smtp:Port`, `Email:Smtp:From` | сервер и отправитель; по умолчанию порт 587 |
| `Email:Smtp:Secure` | `true`: TLS при подключении на любом порту; иначе 465 - TLS при подключении, 587 - STARTTLS |
| `Email:Smtp:Username`, `Email:Smtp:Password` | только для сервера, который требует входа, из хранилища секретов |
| `Email:GraphApi:TenantId`, `ClientId`, `ClientSecret` | приложение, которое отправляет, секрет из хранилища секретов |
| `Email:GraphApi:SenderEmail` | ящик организации, от имени которого уходят письма |
| `Email:Sandbox:Enabled` | по умолчанию `true`; вне Production песочница действует при любом значении |
| `Email:Sandbox:ReceiverAddress` | куда уходят перехваченные письма |

Лежащий почтовый сервер не выводит сервис из трафика: у почты нет проверки готовности, а неудачная
доставка роняет только тот вызов, который её запросил.

Под docker compose инфраструктура получает [MailHog](https://github.com/mailhog/MailHog): он ловит все письма
и показывает их на `http://127.0.0.1:8025`; `SMTP_HOST` и остальные `SMTP_*` и `EMAIL_*` в
`deploy/compose/.env` называют вместо него настоящий сервер.

## Тесты

Тест сервиса даёт ему двойник `INotificationEmailSender`, как любого другого порта. `Common.Tests` проверяет
ядро: какие настройки обязательны, транспорт названного провайдера, что SMTP- и Graph-транспорты передают
клиентам, защиту по порту и песочницу; и на настоящем MailHog из `TEST_SMTP` и `TEST_SMTP_API` - что письмо
в Production приходит получателю, а в других окружениях на адрес песочницы.

```bash
docker run -d --name tests-smtp -p 1025:1025 -p 8025:8025 mailhog/mailhog:v1.0.1
TEST_SMTP=localhost:1025 TEST_SMTP_API=http://localhost:8025/ dotnet test --filter "FullyQualifiedName~Smtp"
```
