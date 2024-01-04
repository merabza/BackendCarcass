using System.Collections.Generic;
using SystemToolsShared;

namespace CarcassMasterDataDom.CellModels;

public /*open*/ class Cell
{
    protected Cell(string typeName, string fieldName, string? caption, bool visible = true)
    {
        TypeName = typeName;
        FieldName = fieldName;
        Caption = caption;
        Visible = visible;
    }

    public string TypeName { get; set; }
    public string FieldName { get; set; }
    public string? Caption { get; set; }
    public bool Visible { get; set; }

    public static NumberCell Number(string fieldName, string? caption, bool visible = true)
    {
        return NumberCell.Create(fieldName, caption, visible);
    }

    public static IntegerCell Integer(string fieldName, string? caption, string? errorCode = null,
        string? errorMessage = null, bool visible = true)
    {
        return IntegerCell.Create(fieldName, caption, errorCode, errorMessage, visible);
    }

    public static RsLookupCell RsLookup(string fieldName, string? caption, string rowSource, string? errorCode = null,
        string? errorMessage = null, bool visible = true)
    {
        return RsLookupCell.Create(fieldName, caption, rowSource, errorCode, errorMessage, visible);
    }

    public static LookupCell Lookup(string fieldName, string? caption, string dataMember, string valueMember,
        string displayMember, string? errorCode = null, string? errorMessage = null, bool visible = true)
    {
        return LookupCell.Create(fieldName, caption, dataMember, valueMember, displayMember, errorCode, errorMessage,
            visible);
    }

    public static MdLookupCell MdLookup(string fieldName, string? caption, string dtTable, string? errorCode = null,
        string? errorMessage = null, bool visible = true)
    {
        return MdLookupCell.Create(fieldName, caption, dtTable, errorCode, errorMessage, visible);
    }

    public static BooleanCell Boolean(string fieldName, string? caption, bool visible = true)
    {
        return BooleanCell.Create(fieldName, caption, visible);
    }

    public static DateCell Date(string fieldName, string? caption, bool showDate = true, bool showTime = true,
        bool visible = true)
    {
        return DateCell.Create(fieldName, caption, showDate, showTime, visible);
    }

    public static StringCell String(string fieldName, string? caption, bool visible = true)
    {
        return StringCell.Create(fieldName, caption, visible);
    }

    public virtual List<Err> Validate(object? value)
    {
        return new List<Err>();
    }
}