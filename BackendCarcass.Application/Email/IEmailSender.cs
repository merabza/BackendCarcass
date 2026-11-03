using System.Threading;
using System.Threading.Tasks;

namespace BackendCarcass.Application.Email;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string toName, string subject, string body,
        CancellationToken cancellationToken = default);
}
