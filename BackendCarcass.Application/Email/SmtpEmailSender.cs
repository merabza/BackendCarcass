using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace BackendCarcass.Application.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        string smtpServer = _settings.SmtpServer ??
                            throw new InvalidOperationException("EmailSettings:SmtpServer is not configured");
        string senderAddress = _settings.SenderEmailAddress ??
                               throw new InvalidOperationException(
                                   "EmailSettings:SenderEmailAddress is not configured");

        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName ?? senderAddress, senderAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Plain) { Text = body };

        //ყურადღება: SmtpClient აქ MailKit-ისაა და არა System.Net.Mail-ის
        using var client = new SmtpClient();
        await client.ConnectAsync(smtpServer, _settings.SmtpPort, SecureSocketOptions.Auto, cancellationToken);
        client.AuthenticationMechanisms.Remove("XOAUTH2");
        if (!string.IsNullOrEmpty(_settings.SmtpUsername))
        {
            await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword ?? string.Empty,
                cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
