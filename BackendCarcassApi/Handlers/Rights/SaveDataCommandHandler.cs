using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.Rights;
using CarcassRepositories;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SaveDataCommandHandler : ICommandHandler<SaveDataCommandRequest, bool>
{
    private readonly IMenuRightsRepository _repository;

    public SaveDataCommandHandler(IMenuRightsRepository mdRepo)
    {
        _repository = mdRepo;
    }

    public async Task<OneOf<bool, IEnumerable<Err>>> Handle(
        SaveDataCommandRequest request, CancellationToken cancellationToken)
    {
        //CurrentUserId;//უნდა იყოს გამოყენებული
        //!!!გასაკეთებელია ის, რომ შენახვისას უნდა შემოწმდეს, ჰქონდა თუ არა უფლება მიმდინარე მომხმარებელს
        //შესაბამისი ინფორმაცია შეენახა
        return await _repository.SaveRightsChanges(request.HttpRequest.HttpContext.User.Identity!.Name!,
            request.ChangesForSave);
    }
}