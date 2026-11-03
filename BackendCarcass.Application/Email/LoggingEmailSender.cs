using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BackendCarcass.Application.Email;

//Development გარემოსთვის, როცა SMTP სერვერი არ არის კონფიგურირებული:
//წერილი არ იგზავნება, მისი შიგთავსი (კოდის და ბმულის ჩათვლით) ლოგში იწერება
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string toName, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "SMTP is not configured, e-mail was not sent. To: {ToName} <{ToEmail}>, Subject: {Subject}, Body:\n{Body}",
            toName, toEmail, subject, body);
        return Task.CompletedTask;
    }
}
