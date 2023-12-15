//using System.ComponentModel.DataAnnotations.Schema;
//using CarcassMasterDataDom;

//namespace CarcassDb.QueryModels;

//public sealed class DataTypeToCrudTypeModel(int dtctId, string dtctKey, string dtctName, int dataTypeId) : IDataType
//{
//    public int DtctId { get; set; } = dtctId;
//    public string DtctKey { get; set; } = dtctKey;
//    public string DtctName { get; set; } = dtctName;
//    public int DataTypeId { get; set; } = dataTypeId;

//    [NotMapped]
//    public int Id
//    {
//        get => DtctId;
//        set => DtctId = value;
//    }

//    [NotMapped] public string Key => DtctKey;

//    [NotMapped] public string Name => DtctName;

//    [NotMapped] public int? ParentId => DataTypeId;

//    public bool UpdateTo(IDataType data)
//    {
//        if (data is not DataTypeToCrudTypeModel newData)
//            return false;
//        DtctKey = newData.DtctKey;
//        DtctName = newData.DtctName;
//        return true;
//    }

//    public dynamic EditFields()
//    {
//        return new { DtctId, DtctKey, DtctName };
//    }
//}

