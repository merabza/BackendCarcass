using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Email;

namespace BackendCarcass.Application.Tests.TestInfrastructure;

public sealed class CapturingEmailSender : IEmailSender
{
    public List<(string ToEmail, string ToName, string Subject, string Body)> Sent { get; } = [];

    public (string ToEmail, string ToName, string Subject, string Body) Last => Sent[^1];

    public Task SendAsync(string toEmail, string toName, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        Sent.Add((toEmail, toName, subject, body));
        return Task.CompletedTask;
    }
}
