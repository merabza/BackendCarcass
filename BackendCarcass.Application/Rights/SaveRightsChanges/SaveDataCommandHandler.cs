using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.Rights;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.DomainShared.Repositories;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Rights.SaveRightsChanges;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SaveDataCommandHandler : ICommandHandler<SaveDataRequestCommand, bool>
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SaveDataCommandHandler> _logger;
    private readonly IRightsRepository _repo;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SaveDataCommandHandler(ILogger<SaveDataCommandHandler> logger, IRightsRepository repo,
        IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OneOf<bool, Err[]>> Handle(SaveDataRequestCommand request, CancellationToken cancellationToken)
    {
        var rightsSaver = new RightsSaver(_logger, _repo, _unitOfWork);
        return await rightsSaver.SaveRightsChanges(_currentUser.Name, request.ChangesForSave, cancellationToken);
    }
}