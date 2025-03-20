using System.Collections.Generic;

namespace CarcassMasterDataDom;

//public /*open*/ class CarcassDataTypeKeys : IDataTypeKeys
//{
//    public CarcassDataTypeKeys()
//    {
//        DtKeys = new Dictionary<string, string>
//        {
//            { ECarcassDataTypeKeys.AppClaim.ToString(), "acl" },
//            { ECarcassDataTypeKeys.CrudRightType.ToString(), "crt" },
//            { ECarcassDataTypeKeys.DataType.ToString(), "dt" },
//            { ECarcassDataTypeKeys.DataTypeToDataType.ToString(), "dtdt" },
//            { ECarcassDataTypeKeys.DataTypeToCrudType.ToString(), "dtct" },
//            { ECarcassDataTypeKeys.MenuGroup.ToString(), "meng" },
//            { ECarcassDataTypeKeys.MenuItm.ToString(), "men" },
//            { ECarcassDataTypeKeys.Role.ToString(), "rol" },
//            { ECarcassDataTypeKeys.User.ToString(), "usr" }
//        };
//    }

//    public Dictionary<string, string> DtKeys { get; }

//    public string GetDtKey(ECarcassDataTypeKeys carcassDataTypeKey)
//    {
//        return DtKeys[carcassDataTypeKey.ToString()];
//    }
//}

public sealed class CarcassDataTypeKeys
{
    private static CarcassDataTypeKeys? _instance;
    private static readonly object SyncRoot = new();

    private readonly Dictionary<string, string> _dtKeys;

    private CarcassDataTypeKeys()
    {
        _dtKeys = new Dictionary<string, string>
        {
            { ECarcassDataTypeKeys.AppClaim.ToString(), "acl" },
            { ECarcassDataTypeKeys.CrudRightType.ToString(), "crt" },
            { ECarcassDataTypeKeys.DataType.ToString(), "dt" },
            { ECarcassDataTypeKeys.DataTypeToDataType.ToString(), "dtdt" },
            { ECarcassDataTypeKeys.DataTypeToCrudType.ToString(), "dtct" },
            { ECarcassDataTypeKeys.MenuGroup.ToString(), "meng" },
            { ECarcassDataTypeKeys.MenuItm.ToString(), "men" },
            { ECarcassDataTypeKeys.Role.ToString(), "rol" },
            { ECarcassDataTypeKeys.User.ToString(), "usr" }
        };
    }

    public static CarcassDataTypeKeys Instance
    {
        get
        {
            //ეს ატრიბუტები სესიაზე არ არის დამოკიდებული და იქმნება პროგრამის გაშვებისთანავე, 
            //შემდგომში მასში ცვლილებები არ შედის,
            //მაგრამ შეიძლება პროგრამამ თავისი მუშაობის განმავლობაში რამდენჯერმე გამოიყენოს აქ არსებული ინფორმაცია
            if (_instance is not null)
                return _instance;
            lock (SyncRoot) //thread safe singleton
            {
                _instance ??= new CarcassDataTypeKeys();
            }

            return _instance;
        }
    }

    public string GetDtKey(ECarcassDataTypeKeys carcassDataTypeKey)
    {
        return _dtKeys[carcassDataTypeKey.ToString()];
    }
}