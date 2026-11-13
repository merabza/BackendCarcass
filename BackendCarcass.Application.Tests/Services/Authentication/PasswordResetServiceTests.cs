using System;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Tests.TestInfrastructure;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.Services.Authentication;

//„დაგავიწყდა პაროლი?“ უცნობ მომხმარებელს წერილს არ უგზავნის და ამას ლოგში წერს, ოღონდ ფორმაში აკრეფილი
//მნიშვნელობის გარეშე: ის შეიძლება სხვისი ელფოსტა იყოს
public sealed class PasswordResetServiceTests
{
    [Fact]
    public async Task An_unknown_user_name_or_email_is_logged_without_the_typed_value()
    {
        using var harness = new LoginServicesHarness();
        var logger = new CapturingLogger<PasswordResetService>();
        var service = new PasswordResetService(harness.UserMgr.Object, harness.EmailVerification,
            new PasswordService(harness.UserMgr.Object, harness.RefreshTokens), harness.CreateCompletion(), logger);

        Result result = await service.ForgotPasswordAsync("someone.else@example.org");

        Assert.True(result.IsSuccess);
        Assert.Empty(harness.Sender.Sent);
        CapturedLogEntry entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.DoesNotContain("someone.else", entry.FullText, StringComparison.OrdinalIgnoreCase);
    }
}
