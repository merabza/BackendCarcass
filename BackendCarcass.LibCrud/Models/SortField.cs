using System;

namespace BackendCarcass.LibCrud.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SortField
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SortField(bool ascending, string fieldName)
    {
        Ascending = ascending;
        FieldName = fieldName;
    }

    public bool Ascending { get; init; }
    public string FieldName { get; set; }
    public Type? PropObjType { get; set; }
}