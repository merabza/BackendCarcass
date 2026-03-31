using System;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.Validation;

public sealed class DateRule
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DateRule(DateTime val, string errCode, string errMessage)
    {
        Val = val;
        Err = new Error { Code = errCode, Name = errMessage };
    }

    public DateTime Val { get; set; }
    public Error Err { get; set; }
}
