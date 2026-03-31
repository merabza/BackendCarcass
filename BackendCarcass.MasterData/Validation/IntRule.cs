using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.Validation;

public sealed class IntRule
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public IntRule(int val, string errCode, string errMessage)
    {
        Val = val;
        Err = new Error { Code = errCode, Name = errMessage };
    }

    public int Val { get; set; }
    public Error Err { get; set; }
}
