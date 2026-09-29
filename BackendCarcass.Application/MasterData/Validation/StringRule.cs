using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData.Validation;

public sealed class StringRule
{
    public StringRule(string val, string errCode, string errMessage)
    {
        Val = val;
        Error = Error.Problem(errCode, errMessage);
    }

    public string Val { get; set; }
    public Error Error { get; set; }
}
