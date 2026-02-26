using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.DataTypes.GetDataTypesList;

public sealed class DataTypesRequestQuery : IQuery<DataTypesResponse[]>;
