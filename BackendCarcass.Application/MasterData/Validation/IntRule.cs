using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData.Validation;

public sealed class IntRule
{
    public IntRule(int val, string errCode, string errMessage)
    {
        Val = val;
        Error = Error.Problem(errCode, errMessage);
    }

    public int Val { get; set; }
    public Error Error { get; set; }
}
