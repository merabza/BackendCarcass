namespace CarcassMasterDataDom.Models;

public record DataTypeModelForRvs(
    int DtId,
    string DtTable,
    string DtName,
    string? DtIdFieldName,
    string? DtKeyFieldName,
    string? DtNameFieldName,
    int? DtParentDataTypeId,
    int? DtManyToManyJoinParentDataTypeId,
    int? DtManyToManyJoinChildDataTypeId);