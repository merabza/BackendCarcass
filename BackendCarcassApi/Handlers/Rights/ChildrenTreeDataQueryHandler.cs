using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.Rights;
using CarcassDb.QueryModels;
using CarcassRepositories;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChildrenTreeDataQueryHandler : ICommandHandler<ChildrenTreeDataCommandRequest, List<DataTypeModel>>
{
    private readonly IMenuRightsRepository _mdRepo;

    public ChildrenTreeDataQueryHandler(IMenuRightsRepository mdRepo)
    {
        _mdRepo = mdRepo;
    }

    public async Task<OneOf<List<DataTypeModel>, IEnumerable<Err>>> Handle(
        ChildrenTreeDataCommandRequest request, CancellationToken cancellationToken)
    {
        var dataTypeModels = await _mdRepo.ChildrenTreeData(request.HttpRequest.HttpContext.User.Identity!.Name!,
            request.dataTypeKey, request.ViewStyle, cancellationToken);
        return dataTypeModels;
    }
}