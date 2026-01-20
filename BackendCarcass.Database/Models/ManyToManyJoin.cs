using System;

namespace BackendCarcass.Database.Models;

public sealed class ManyToManyJoin
{
    public int MmjId { get; init; }
    public int PtId { get; init; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string PKey { get; init; }
    public int CtId { get; init; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string CKey { get; init; }

    public DataType ParentDataTypeNavigation
    {
        get =>
            field ?? throw new InvalidOperationException("Uninitialized property: " + nameof(ParentDataTypeNavigation));
        init;
    }

    public DataType ChildDataTypeNavigation
    {
        get =>
            field ?? throw new InvalidOperationException("Uninitialized property: " + nameof(ChildDataTypeNavigation));
        init;
    }
}