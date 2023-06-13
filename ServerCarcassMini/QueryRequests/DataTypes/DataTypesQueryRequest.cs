using CarcassContracts.V1.Responses;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace ServerCarcassMini.QueryRequests.DataTypes;

public sealed class DataTypesQueryRequest : IQuery<DataTypesResponse[]>
{
    public DataTypesQueryRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; }
}