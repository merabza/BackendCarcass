using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CarcassContracts.ErrorModels;
using CarcassContracts.V1.Responses;
using CarcassRepositories;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;
using OneOf;
using ServerCarcassMini.Handlers.Authentication;
using ServerCarcassMini.QueryRequests.DataTypes;
using SystemToolsShared;

namespace ServerCarcassMini.Handlers.DataTypes;

// ReSharper disable once UnusedType.Global
public sealed class DataTypesListQueryHandler : LoginCommandBase,
    IQueryHandler<DataTypesQueryRequest, DataTypesResponse[]>
{
    private readonly IMenuRightsRepository _repository;

    public DataTypesListQueryHandler(IMenuRightsRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<DataTypesResponse[], IEnumerable<Err>>> Handle(DataTypesQueryRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserName = CurrentUserName(request.HttpRequest);
        if (currentUserName is null)
            return new[] { CarcassApiErrors.InvalidUser };
        var res = await _repository.DataTypes(currentUserName);
        return res;
    }

    private static string? CurrentUserName(HttpRequest request)
    {
        return request.HttpContext.User.Claims.SingleOrDefault(so => so.Type == ClaimTypes.Name)?.Value;
    }
}