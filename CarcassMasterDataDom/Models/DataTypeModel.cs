namespace CarcassMasterDataDom.Models;

public record DataTypeModel(int DtId, string DtKey, string DtName, string DtNameNominative, string DtNameGenitive,
    string DtTable, string DtIdFieldName, string? DtKeyFieldName, string? DtNameFieldName, int? DtParentDataTypeId,
    int? DtManyToManyJoinParentDataTypeId, int? DtManyToManyJoinChildDataTypeId);
