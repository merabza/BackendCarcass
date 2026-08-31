using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using Microsoft.Extensions.Logging;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Application.Rights.SaveRightsChanges;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SaveDataCommandHandler(
    ILogger<SaveDataCommandHandler> logger,
    IRightsRepository repo,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDatabaseAbstraction databaseAbstraction) : ICommandHandler<SaveDataRequestCommand, bool>
{
    public async Task<Result<bool>> Handle(SaveDataRequestCommand request, CancellationToken cancellationToken)
    {
        var rightsSaver = new RightsSaver(logger, repo, unitOfWork, databaseAbstraction);
        return await rightsSaver.SaveRightsChanges(currentUser.Name, request.ChangesForSave, cancellationToken);
    }
}
