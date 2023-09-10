using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.Rights;
using CarcassRepositories;
using CarcassRepositories.Models;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HalfChecksQueryHandler : ICommandHandler<HalfChecksCommandRequest, List<TypeDataModel>>
{
    private readonly IMenuRightsRepository _repository;

    public HalfChecksQueryHandler(IMenuRightsRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<List<TypeDataModel>, IEnumerable<Err>>> Handle(
        HalfChecksCommandRequest request, CancellationToken cancellationToken)
    {
        var typeDataModels = await _repository.HalfChecks(request.HttpRequest.HttpContext.User.Identity!.Name!,
            request.DataTypeId, request.DataKey, request.ViewStyle, cancellationToken);
        return typeDataModels;
    }
}