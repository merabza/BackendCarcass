using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Email;
using BackendCarcass.Application.Tests.TestInfrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace BackendCarcass.Application.Tests.Email;

//SmtpEmailSender წერილს ნამდვილი MailKit კლიენტით უგზავნის ტესტის SMTP სერვერს. ლოგში რჩება თემა, სერვერი, პორტი
//და სერვერის პასუხი, მაგრამ არ იწერება მიმღების მისამართი და Message-Id (SsoLtgGe-ის გეგმის მე-7 წესი)
public sealed class SmtpEmailSenderTests
{
    private const string Recipient = "private.recipient@example.com";
    private const string Subject = "App: პაროლის აღდგენა";

    [Fact]
    public async Task The_log_keeps_the_subject_server_and_queue_response_but_not_the_recipient_or_message_id()
    {
        using var server = new FakeSmtpServer();
        var logger = new CapturingLogger<SmtpEmailSender>();
        var sender = new SmtpEmailSender(Options.Create(new EmailSettings
        {
            SmtpServer = "127.0.0.1",
            SmtpPort = server.Port,
            SenderEmailAddress = "noreply@app.test",
            SenderName = "App"
        }), logger);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await sender.SendAsync(Recipient, "ტესტ მომხმარებელი", Subject, "კოდი: 123456", timeout.Token);

        //წერილი ნამდვილად ამ მისამართზე წავიდა და Message-Id ჰქონდა, ამიტომ ლოგში მათი არარსებობა შემთხვევითი არ არის
        string received = await server.Message.WaitAsync(timeout.Token);
        Assert.Contains(Recipient, received, StringComparison.Ordinal);
        string messageId = Regex.Match(received, "^Message-Id:\\s*<([^>]+)>",
            RegexOptions.Multiline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)).Groups[1].Value;
        Assert.NotEmpty(messageId);

        string serverAndPort = string.Create(CultureInfo.InvariantCulture, $"127.0.0.1:{server.Port}");
        Assert.Collection(logger.Entries,
            sending =>
            {
                Assert.Equal(LogLevel.Information, sending.Level);
                Assert.Contains(Subject, sending.Message, StringComparison.Ordinal);
                Assert.Contains(serverAndPort, sending.Message, StringComparison.Ordinal);
            },
            accepted =>
            {
                Assert.Equal(LogLevel.Information, accepted.Level);
                Assert.Contains(Subject, accepted.Message, StringComparison.Ordinal);
                Assert.Contains("127.0.0.1", accepted.Message, StringComparison.Ordinal);
                Assert.Contains(FakeSmtpServer.QueuedResponse, accepted.Message, StringComparison.Ordinal);
            });
        Assert.All(logger.Entries, entry =>
        {
            Assert.DoesNotContain(Recipient, entry.FullText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private.recipient", entry.FullText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(messageId, entry.FullText, StringComparison.OrdinalIgnoreCase);
        });
    }
}
