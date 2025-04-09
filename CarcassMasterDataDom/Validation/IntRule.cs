using SystemToolsShared.Errors;

namespace CarcassMasterDataDom.Validation;

public sealed class IntRule
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public IntRule(int val, string errCode, string errMessage)
    {
        Val = val;
        Err = new Err { ErrorCode = errCode, ErrorMessage = errMessage };
    }

    public int Val { get; set; }
    public Err Err { get; set; }
}