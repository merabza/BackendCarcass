using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Authentication.Logout;

//სისტემიდან გასვლისას განახლების ტოკენი უქმდება. წვდომის ტოკენი თავისი ვადის ამოწურვამდე ტექნიკურად მოქმედია
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LogoutCommandHandler(RefreshTokenService refreshTokenService)
    : ICommandHandler<LogoutRequestCommand>
{
    public async Task<Result> Handle(LogoutRequestCommand request, CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        return Result.Success();
    }
}
