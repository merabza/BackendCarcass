namespace CarcassMasterDataDom.Models;

public record DataTypeModelForRvs(int DtId, string DtKey, string DtName, string DtTable, string? DtIdFieldName,
    string? DtKeyFieldName, string? DtNameFieldName, int? DtParentDataTypeId, int? DtManyToManyJoinParentDataTypeId,
    int? DtManyToManyJoinChildDataTypeId);
