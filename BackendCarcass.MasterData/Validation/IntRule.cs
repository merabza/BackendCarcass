using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.Validation;

public sealed class IntRule
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public IntRule(int val, string errCode, string errMessage)
    {
        Val = val;
        ErrorOmd = new ErrorOmd { Code = errCode, Name = errMessage };
    }

    public int Val { get; set; }
    public ErrorOmd ErrorOmd { get; set; }
}
