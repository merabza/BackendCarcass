using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.Models;

//ეს კლასი არის მონაცემთა ბაზაში ინფორმაციის ცვლილების შესაბამისი ოპერაციების ცხრილის ჩანაწერის მოდელი
//ცხრილი გამოიყენება ბაზაში ინფორმაციის ცვლილების უფლებების ტიპების სახელების შესანახად.
//ეს შეიძლება იყოს დამატება, რედაქტირება, წაშლა, დადასტურება
//ნახვა აქ არ მოხვდება, რადგან თუ ცხრილზე ან სხვა ტიპის ინფორმაციაზე უფლება არსებობს,
//  ეს უკვე ნიშნავს, რომ ნახვის უფლება არსებობს
public sealed class CrudRightType : IDataType, IMyEquatable
{
    public int CrtId { get; set; } //იდენტიფიკატორი
    [MaxLength(50)] public required string CrtKey { get; set; } //კოდი
    [MaxLength(50)] public required string CrtName { get; set; } //სახელი

    [NotMapped]
    public int Id
    {
        get => CrtId;
        set => CrtId = value;
    }

    [NotMapped] public string Key => CrtKey;

    [NotMapped] public string Name => CrtName;

    [NotMapped] public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        if (data is not CrudRightType newData)
            return false;
        CrtKey = newData.CrtKey;
        CrtName = newData.CrtName;
        return true;
    }

    public dynamic EditFields()
    {
        return new { CrtId, CrtKey, CrtName };
    }

    public bool EqualsTo(IDataType data)
    {
        if (data is not CrudRightType other)
            return false;

        return CrtKey == other.CrtKey && CrtName == other.CrtName;
    }
}