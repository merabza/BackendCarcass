using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.Rights;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.Domain.Abstractions;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Rights.SaveRightsChanges;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SaveDataCommandHandler(
    ILogger<SaveDataCommandHandler> logger,
    IRightsRepository repo,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDatabaseAbstraction databaseAbstraction) : ICommandHandler<SaveDataRequestCommand, bool>
{
    public async Task<OneOf<bool, Error[]>> Handle(SaveDataRequestCommand request, CancellationToken cancellationToken)
    {
        var rightsSaver = new RightsSaver(logger, repo, unitOfWork, databaseAbstraction);
        return await rightsSaver.SaveRightsChanges(currentUser.Name, request.ChangesForSave, cancellationToken);
    }
}
