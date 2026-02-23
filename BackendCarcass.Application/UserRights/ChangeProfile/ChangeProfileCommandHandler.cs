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

namespace BackendCarcass.Application.UserRights.ChangeProfile;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangeProfileCommandHandler : ICommandHandler<ChangeProfileRequestCommand>
{
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<AppUser> _userMgr;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ChangeProfileCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    {
        _userMgr = userMgr;
        _currentUser = currentUser;
    }

    public async Task<OneOf<Unit, Err[]>> Handle(ChangeProfileRequestCommand request,
        CancellationToken cancellationToken)
    {
        //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
        AppUser? user = await _userMgr.FindByNameAsync(_currentUser.Name);

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
        IdentityResult result = await _userMgr.UpdateAsync(user);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        return !result.Succeeded ? new[] { UserRightsErrors.FailedToSaveUserInformation } : new Unit();
    }
}
