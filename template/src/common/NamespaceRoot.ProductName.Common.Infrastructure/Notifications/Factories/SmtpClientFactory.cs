using MailKit.Net.Smtp;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Notifications.Factories;

internal interface ISmtpClientFactory
{
    ISmtpClient Create();
}

internal sealed class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClient Create() => new SmtpClient();
}
