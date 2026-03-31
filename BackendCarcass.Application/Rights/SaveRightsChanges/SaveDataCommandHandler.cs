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
public sealed class SaveDataCommandHandler : ICommandHandler<SaveDataRequestCommand, bool>
{
    private readonly ICurrentUser _currentUser;
    private readonly IDatabaseAbstraction _databaseAbstraction;
    private readonly ILogger<SaveDataCommandHandler> _logger;
    private readonly IRightsRepository _repo;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SaveDataCommandHandler(ILogger<SaveDataCommandHandler> logger, IRightsRepository repo,
        IUnitOfWork unitOfWork, ICurrentUser currentUser, IDatabaseAbstraction databaseAbstraction)
    {
        _repo = repo;
        _currentUser = currentUser;
        _databaseAbstraction = databaseAbstraction;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OneOf<bool, Error[]>> Handle(SaveDataRequestCommand request, CancellationToken cancellationToken)
    {
        var rightsSaver = new RightsSaver(_logger, _repo, _unitOfWork, _databaseAbstraction);
        return await rightsSaver.SaveRightsChanges(_currentUser.Name, request.ChangesForSave, cancellationToken);
    }
}
