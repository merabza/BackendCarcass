using System;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.MasterData.Validation;

public sealed class DateRule
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DateRule(DateTime val, string errCode, string errMessage)
    {
        Val = val;
        ErrorOmd = new ErrorOmd { Code = errCode, Name = errMessage };
    }

    public DateTime Val { get; set; }
    public ErrorOmd ErrorOmd { get; set; }
}
