namespace CarcassContracts.V1.Routes;

public static class CarcassApiRoutes
{
    private const string Root = "api";
    private const string Version = "v1";
    private const string Base = Root + "/" + Version;

    public static class Authentication
    {
        private const string AuthenticationBase = Base + "/authentication";

        // POST api/v1/authentication/registration
        public const string Registration = AuthenticationBase + "/registration";

        // POST api/v1/authentication/login
        public const string Login = AuthenticationBase + "/login";
    }

    public static class DataTypes
    {
        public const string DataTypesBase = Base + "/datatypes";

        // GET api/v1/dataTypes/getdatatypes
        public const string DataTypesList = "/getdatatypes";

        // GET api/v1/dataTypes/getgridmodel
        public const string GridModel = "/getgridmodel/{gridname}";

        // GET api/v1/dataTypes/getmultiplegridrules
        public const string MultipleGridModels = "/getmultiplegridrules";
    }

    public static class MasterData
    {
        public const string MasterDataBase = Base + "/masterdata";

        // GET api/v1/masterdata/{tableName}
        public const string All = "/{tableName}";

        // GET api/v1/masterdata/gettables
        public const string Tables = "/gettables";

        // GET api/v1/masterdata/gettablerowsdata/{tableName}
        public const string GetTableRowsData = "/gettablerowsdata/{tableName}";

        // GET api/v1/masterdata/{tableName}/{id}
        public const string Get = "/{tableName}/{id}";

        // POST api/v1/masterdata/{tableName}
        public const string Post = "/{tableName}";

        // PUT api/v1/masterdata/{tableName}/{id}
        public const string Put = "/{tableName}/{id}";

        // DELETE api/v1/masterdata/{tableName}/{id}
        public const string Delete = "/{tableName}/{id}";
    }

    public static class Processes
    {
        private const string ProcessesBase = Base + "/processes";

        public const string Status = ProcessesBase + "/getstatus/{commandkey}";
    }

    public static class UserRights
    {
        public const string UserRightsBase = Base + "/userrights";

        public const string IsCurrentUserValid = "/iscurrentuservalid";
        public const string ChangeProfile = "/changeprofile";
        public const string ChangePassword = "/changepassword";
        public const string DeleteCurrentUser = "/deletecurrentuser/{userName}";
        public const string MainMenu = "/getmainmenu";
    }


    public static class Rights
    {
        public const string RightsBase = Base + "/rights";

        public const string ParentsTreeData = "/getparentstreedata/{viewStyle}";
        public const string ChildrenTreeData = "/getchildrentreedata/{dataTypeKey}/{viewStyle}";
        public const string HalfChecks = "/halfchecks/{dataTypeId}/{dataKey}/{viewStyle}";
        public const string SaveData = "/savedata";
        public const string Optimize = "/optimize";
    }
}