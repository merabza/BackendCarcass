using System;

namespace CarcassDb.Models;

public sealed class ManyToManyJoin
{
    private DataType? _childDataTypeNavigation;
    private DataType? _parentDataTypeNavigation;

    public int MmjId { get; set; }
    public int PtId { get; set; }
    public string PKey { get; set; } = null!;
    public int CtId { get; set; }
    public string CKey { get; set; } = null!;

    public DataType ParentDataTypeNavigation
    {
        get => _parentDataTypeNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(ParentDataTypeNavigation));
        set => _parentDataTypeNavigation = value;
    }

    public DataType ChildDataTypeNavigation
    {
        get => _childDataTypeNavigation ??
               throw new InvalidOperationException("Uninitialized property: " + nameof(ChildDataTypeNavigation));
        set => _childDataTypeNavigation = value;
    }
}