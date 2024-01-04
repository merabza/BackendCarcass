using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.Rights;
using CarcassDom;
using MessagingAbstractions;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SaveDataCommandHandler : ICommandHandler<SaveDataCommandRequest, bool>
{
    private readonly ILogger<SaveDataCommandHandler> _logger;

    //private readonly IMenuRightsRepository _repository;
    private readonly IRightsRepository _repo;

    public SaveDataCommandHandler(ILogger<SaveDataCommandHandler> logger, IRightsRepository repo)
    {
        //_repository = mdRepo;
        _repo = repo;
        _logger = logger;
    }

    public async Task<OneOf<bool, IEnumerable<Err>>> Handle(
        SaveDataCommandRequest request, CancellationToken cancellationToken)
    {
        var rightsSaver = new RightsSaver(_logger, _repo);
        return await rightsSaver.SaveRightsChanges(request.HttpRequest.HttpContext.User.Identity!.Name!,
            request.ChangesForSave, cancellationToken);


        ////CurrentUserId;//უნდა იყოს გამოყენებული
        ////!!!გასაკეთებელია ის, რომ შენახვისას უნდა შემოწმდეს, ჰქონდა თუ არა უფლება მიმდინარე მომხმარებელს
        ////შესაბამისი ინფორმაცია შეენახა
        //return await _repository.SaveRightsChanges(request.HttpRequest.HttpContext.User.Identity!.Name!,
        //    request.ChangesForSave, cancellationToken);
    }
}