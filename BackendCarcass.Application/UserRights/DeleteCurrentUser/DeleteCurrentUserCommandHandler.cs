using System.Threading;
using System.Threading.Tasks;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using AppUser = BackendCarcass.Application.MasterData.Models.AppUser;
using ICurrentUser = BackendCarcass.Application.Identity.ICurrentUser;
using UsersMdRepo = BackendCarcass.Application.Repositories.UsersMdRepo;

namespace BackendCarcass.Application.UserRights.DeleteCurrentUser;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DeleteCurrentUserCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    : ICommandHandler<DeleteCurrentUserRequestCommand>
{
    public async Task<Result> Handle(DeleteCurrentUserRequestCommand request, CancellationToken cancellationToken)
    {
        //ეს ერთგვარი ტესტია. თუ კოდი აქამდე მოვიდა, მიმდინარე მომხმარებელი ვალიდურია
        if (currentUser.Name != request.UserName)
        {
            return Result.Failure(UserRightsErrors.BadRequestFailedToDeleteUser);
        }

        var usersMdRepo = new UsersMdRepo(userMgr);
        AppUser? user = await userMgr.FindByNameAsync(request.UserName);
        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
        {
            return Result.Failure(UserRightsErrors.NoUserFound);
        }

        Result deleteResult = await usersMdRepo.Delete(user.Id);
        if (deleteResult.IsSuccess)
        {
            return Result.Success();
        }

        return Result.Failure(UserRightsErrors.DeletionErrorUserCouldNotBeDeleted);
    }
}
