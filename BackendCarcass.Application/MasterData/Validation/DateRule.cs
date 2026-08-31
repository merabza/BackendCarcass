using System;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData.Validation;

public sealed class DateRule
{
    public DateRule(DateTime val, string errCode, string errMessage)
    {
        Val = val;
        Error = Error.Problem(errCode, errMessage);
    }

    public DateTime Val { get; set; }
    public Error Error { get; set; }
}
