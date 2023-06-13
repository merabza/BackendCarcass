//using Newtonsoft.Json;
//using SystemToolsShared;

//namespace CarcassMasterDataDom.Validation;

//public class ValidationRule
//{
//    public ValidationRule(string errCode, string errMessage)
//    {
//        ErrCode = errCode;
//        ErrMessage = errMessage;
//    }

//    [JsonIgnore] public string ErrCode { get; set; }

//    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
//    public string ErrMessage { get; set; }

//    public Err Err => new() { ErrorCode = ErrCode, ErrorMessage = ErrMessage };
//}

