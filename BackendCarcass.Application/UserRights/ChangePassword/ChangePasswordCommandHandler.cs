using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.MasterData.Models;
using BackendCarcassShared.Contracts.Errors;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.UserRights.ChangePassword;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangePasswordCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    : ICommandHandler<ChangePasswordRequestCommand>
{
    public async Task<OneOf<Unit, Error[]>> Handle(ChangePasswordRequestCommand request,
        CancellationToken cancellationToken)
    {
        //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
        AppUser? user = await userMgr.FindByNameAsync(request.UserName!);
        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
        {
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };
        }

        if (user.Id != request.Userid || currentUser.Name != user.UserName)
        {
            return new[] { UserRightsErrors.UserAuthenticationFailedThePasswordHasNotBeenChanged };
        }

        IdentityResult result = await userMgr.ChangePasswordAsync(user, request.OldPassword!, request.NewPassword!);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        return !result.Succeeded ? new[] { UserRightsErrors.FailedToChangePassword } : new Unit();
    }
}
