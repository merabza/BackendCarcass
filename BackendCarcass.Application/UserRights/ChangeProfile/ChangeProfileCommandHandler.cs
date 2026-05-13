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

namespace BackendCarcass.Application.UserRights.ChangeProfile;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangeProfileCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    : ICommandHandler<ChangeProfileRequestCommand>
{
    public async Task<OneOf<Unit, Error[]>> Handle(ChangeProfileRequestCommand request,
        CancellationToken cancellationToken)
    {
        //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
        AppUser? user = await userMgr.FindByNameAsync(currentUser.Name);

        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
        {
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };
        }

        if (user.Id != request.Userid || user.UserName != request.UserName || user.Email != request.Email)
        {
            return new[] { UserRightsErrors.UserNotIdentifierSaveFiled };
        }

        user.FirstName = request.FirstName!;
        user.LastName = request.LastName!;
        IdentityResult result = await userMgr.UpdateAsync(user);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        return !result.Succeeded ? new[] { UserRightsErrors.FailedToSaveUserInformation } : new Unit();
    }
}
