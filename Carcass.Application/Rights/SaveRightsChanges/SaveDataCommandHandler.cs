using System.Threading;
using System.Threading.Tasks;
using CarcassIdentity;
using CarcassRights;
using MediatRMessagingAbstractions;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared.Errors;

namespace Carcass.Application.Rights.SaveRightsChanges;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SaveDataCommandHandler : ICommandHandler<SaveDataRequestCommand, bool>
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SaveDataCommandHandler> _logger;
    private readonly IRightsRepository _repo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SaveDataCommandHandler(ILogger<SaveDataCommandHandler> logger, IRightsRepository repo,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<OneOf<bool, Err[]>> Handle(SaveDataRequestCommand request,
        CancellationToken cancellationToken = default)
    {
        var rightsSaver = new RightsSaver(_logger, _repo);
        return await rightsSaver.SaveRightsChanges(_currentUser.Name, request.ChangesForSave, cancellationToken);
    }
}