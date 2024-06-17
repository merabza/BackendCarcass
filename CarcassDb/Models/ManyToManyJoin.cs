using System;
using System.ComponentModel.DataAnnotations;

namespace CarcassDb.Models;

public sealed class ManyToManyJoin
{
    private DataType? _childDataTypeNavigation;
    private DataType? _parentDataTypeNavigation;

    public int MmjId { get; set; }
    public int PtId { get; set; }
    [MaxLength(100)]
    public required string PKey { get; set; } = null!;
    public int CtId { get; set; }
    [MaxLength(100)]
    public required string CKey { get; set; } = null!;

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