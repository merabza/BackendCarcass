namespace CarcassMasterDataDom.Models;

public sealed class ReturnValueModel
{
    //public ReturnValueModel(int iValue, string? key, string? name, int? parentId)
    //{
    //    IValue = iValue;
    //    Key = key;
    //    Name = name;
    //    ParentId = parentId;
    //}

    public int Id { get; set; }
    public string? Key { get; set; }
    public string? Name { get; set; }
    public int? ParentId { get; set; }
}