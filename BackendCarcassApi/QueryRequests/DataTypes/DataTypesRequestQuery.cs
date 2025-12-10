using BackendCarcassContracts.V1.Responses;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.DataTypes;

public sealed class DataTypesRequestQuery : IQuery<DataTypesResponse[]>
{
}