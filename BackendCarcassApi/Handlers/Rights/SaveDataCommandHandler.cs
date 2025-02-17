using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.Rights;
using CarcassDom;
using CarcassIdentity;
using MessagingAbstractions;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SaveDataCommandHandler : ICommandHandler<SaveDataCommandRequest, bool>
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

    public async Task<OneOf<bool, IEnumerable<Err>>> Handle(SaveDataCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var rightsSaver = new RightsSaver(_logger, _repo);
        return await rightsSaver.SaveRightsChanges(_currentUser.Name, request.ChangesForSave, cancellationToken);
    }
}