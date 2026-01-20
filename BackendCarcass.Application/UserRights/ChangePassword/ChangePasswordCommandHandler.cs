using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.MasterData.Models;
using BackendCarcassContracts.Errors;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.UserRights.ChangePassword;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordRequestCommand>
{
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<AppUser> _userMgr;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ChangePasswordCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    {
        _userMgr = userMgr;
        _currentUser = currentUser;
    }

    public async Task<OneOf<Unit, Err[]>> Handle(ChangePasswordRequestCommand request,
        CancellationToken cancellationToken)
    {
        //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
        var user = await _userMgr.FindByNameAsync(request.UserName!);
        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null) return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        if (user.Id != request.Userid || _currentUser.Name != user.UserName)
            return new[] { UserRightsErrors.UserAuthenticationFailedThePasswordHasNotBeenChanged };

        var result = await _userMgr.ChangePasswordAsync(user, request.OldPassword!, request.NewPassword!);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        return !result.Succeeded ? new[] { UserRightsErrors.FailedToChangePassword } : new Unit();
    }
}