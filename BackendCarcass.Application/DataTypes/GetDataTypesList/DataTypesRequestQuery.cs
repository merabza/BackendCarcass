using BackendCarcassContracts.V1.Responses;
using MediatRMessagingAbstractions;

namespace Carcass.Application.DataTypes.GetDataTypesList;

public sealed class DataTypesRequestQuery : IQuery<DataTypesResponse[]>
{
}