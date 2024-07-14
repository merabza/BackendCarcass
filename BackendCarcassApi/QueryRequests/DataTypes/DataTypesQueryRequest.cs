using BackendCarcassContracts.V1.Responses;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.QueryRequests.DataTypes;

public sealed class DataTypesQueryRequest : IQuery<DataTypesResponse[]>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DataTypesQueryRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; }
}