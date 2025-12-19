using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.Models;

//მონაცემთა ტიპი
public sealed class DataType : IDataType, IMyEquatable
{
    //public static string DtKeyKey => nameof(DtId).CountDtKey();

    public int DtId { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    //public required string DtKey { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string DtName { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string DtNameNominative { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string DtNameGenitive { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string DtTable { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? DtIdFieldName { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? DtKeyFieldName { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? DtNameFieldName { get; set; }
    public int? DtParentDataTypeId { get; set; }
    public int? DtManyToManyJoinParentDataTypeId { get; set; }
    public int? DtManyToManyJoinChildDataTypeId { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? DtGridRulesJson { get; set; }

    public DataType? DtParentDataTypeNavigation { get; set; }
    public DataType? DtManyToManyJoinParentDataTypeNavigation { get; set; }
    public DataType? DtManyToManyJoinChildDataTypeNavigation { get; set; }

    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<ManyToManyJoin> ManyToManyJoinParentTypes { get; set; } = new List<ManyToManyJoin>();

    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<ManyToManyJoin> ManyToManyJoinChildTypes { get; set; } = new List<ManyToManyJoin>();

    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<DataType> ChildrenDataTypes { get; set; } = new List<DataType>();

    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<DataType> ManyJoinParentDataTypes { get; set; } = new List<DataType>();

    // ReSharper disable once CollectionNeverUpdated.Global
    public ICollection<DataType> ManyToManyJoinChildrenDataTypes { get; set; } = new List<DataType>();

    [NotMapped]
    public int Id
    {
        get => DtId;
        set => DtId = value;
    }

    [NotMapped] public string Key => DtTable;

    [NotMapped] public string Name => DtName;

    [NotMapped] public int? ParentId => null;

    public dynamic EditFields()
    {
        return new
        {
            DtId,
            //DtKey,
            DtTable,
            DtName,
            DtNameNominative,
            DtNameGenitive,
            DtIdFieldName,
            DtKeyFieldName,
            DtNameFieldName,
            DtParentDataTypeId
        };
    }

    public bool UpdateTo(IDataType data)
    {
        if (data is not DataType newData)
            return false;
        //DtKey = newData.DtKey;
        DtName = newData.DtName;
        DtNameNominative = newData.DtNameNominative;
        DtNameGenitive = newData.DtNameGenitive;
        DtTable = newData.DtTable;
        DtIdFieldName = newData.DtIdFieldName;
        DtKeyFieldName = newData.DtKeyFieldName;
        DtNameFieldName = newData.DtNameFieldName;
        DtParentDataTypeId = newData.DtParentDataTypeId;
        DtGridRulesJson = newData.DtGridRulesJson;
        return true;
    }

    public bool EqualsTo(IDataType data)
    {
        if (data is not DataType other)
            return false;

        return
            //DtKey == other.DtKey && 
            DtName == other.DtName && DtNameNominative == other.DtNameNominative &&
            DtNameGenitive == other.DtNameGenitive && DtTable == other.DtTable &&
            DtIdFieldName == other.DtIdFieldName && Equals(DtKeyFieldName, other.DtKeyFieldName) &&
            Equals(DtNameFieldName, other.DtNameFieldName) && Equals(DtParentDataTypeId, other.DtParentDataTypeId) &&
            Equals(DtGridRulesJson, other.DtGridRulesJson);
    }
}