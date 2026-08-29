using System.Threading;
using System.Threading.Tasks;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using AppUser = BackendCarcass.Application.MasterData.Models.AppUser;
using ICurrentUser = BackendCarcass.Application.Identity.ICurrentUser;

namespace BackendCarcass.Application.UserRights.ChangeProfile;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangeProfileCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    : ICommandHandler<ChangeProfileRequestCommand>
{
    public async Task<Result> Handle(ChangeProfileRequestCommand request, CancellationToken cancellationToken)
    {
        //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
        AppUser? user = await userMgr.FindByNameAsync(currentUser.Name);

        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
        {
            return Result.Failure(AuthenticationApiErrors.UsernameOrPasswordIsIncorrect);
        }

        if (user.Id != request.Userid || user.UserName != request.UserName || user.Email != request.Email)
        {
            return Result.Failure(UserRightsErrors.UserNotIdentifierSaveFiled);
        }

        user.FirstName = request.FirstName!;
        user.LastName = request.LastName!;
        IdentityResult result = await userMgr.UpdateAsync(user);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        return !result.Succeeded ? Result.Failure(UserRightsErrors.FailedToSaveUserInformation) : Result.Success();
    }
}
