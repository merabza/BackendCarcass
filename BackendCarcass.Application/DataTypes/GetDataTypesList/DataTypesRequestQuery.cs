using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.DataTypes.GetDataTypesList;

public sealed class DataTypesRequestQuery : IQuery<DataTypesResponse[]>;
