﻿using SystemToolsShared;

namespace CarcassMasterDataDom.Validation;

public class IntRule
{
    public IntRule(int val, string errCode, string errMessage)
    {
        Val = val;
        Err = new Err { ErrorCode = errCode, ErrorMessage = errMessage };
    }

    public int Val { get; set; }
    public Err Err { get; set; }
}