using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using AppUser = BackendCarcass.Application.MasterData.Models.AppUser;
using ICurrentUser = BackendCarcass.Application.Identity.ICurrentUser;

namespace BackendCarcass.Application.UserRights.ChangePassword;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangePasswordCommandHandler(
    UserManager<AppUser> userMgr,
    ICurrentUser currentUser,
    RefreshTokenService refreshTokenService) : ICommandHandler<ChangePasswordRequestCommand>
{
    public async Task<Result> Handle(ChangePasswordRequestCommand request, CancellationToken cancellationToken)
    {
        //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
        AppUser? user = await userMgr.FindByNameAsync(request.UserName!);
        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
        {
            return Result.Failure(AuthenticationApiErrors.UsernameOrPasswordIsIncorrect);
        }

        if (user.Id != request.Userid || currentUser.Name != user.UserName)
        {
            return Result.Failure(UserRightsErrors.UserAuthenticationFailedThePasswordHasNotBeenChanged);
        }

        IdentityResult result = await userMgr.ChangePasswordAsync(user, request.OldPassword!, request.NewPassword!);
        //თუ პაროლის ცვლილებისას წარმოიშვა პრობლემა (ძველი პაროლი არასწორია, ან ახალი წესებს არ აკმაყოფილებს), ვჩერდებით
        if (!result.Succeeded)
        {
            return Result.Failure(result.ToError(UserRightsErrors.FailedToChangePassword));
        }

        //პაროლი შეიცვალა: ძველი პაროლით გახსნილი ყველა სესია უქმდება.
        //მიმდინარე წვდომის ტოკენი თავისი ვადის ამოწურვამდე მოქმედებს, შემდეგ ავტორიზაცია თავიდან იქნება საჭირო
        await refreshTokenService.RevokeAllAsync(user.Id, cancellationToken);
        return Result.Success();
    }
}
