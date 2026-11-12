using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Rights;
using BackendCarcass.Application.Rights.Models;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Domain.Roles;
using BackendCarcass.Domain.Users;
using Moq;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using Xunit;

namespace BackendCarcass.Application.Tests.Rights;

// The rights editor lists a user without roles, so that an admin can give them a first role (BackendCarcass#7), and
// still hides a user whose role outranks the viewer's (a lower level number); in every tree and view style
public sealed class RightsCollectorUsersTests : IDisposable
{
    private const string UsersTable = "Users";
    private const string RolesTable = "Roles";
    private const string DataTypesTable = "DataTypes";
    private const int UsersDtId = 1;
    private const int RolesDtId = 2;
    private const int DataTypesDtId = 3;
    private const int ViewerLevel = 10;

    private const int RoleLessUserId = 1;
    private const int HigherRankedUserId = 2;
    private const int SameLevelUserId = 3;
    private const int LowerRankedUserId = 4;

    private readonly CarcassSqliteDatabase _db = new();
    private readonly Mock<IRightsRepository> _repo = new();
    private readonly RightsCollector _sut;

    public RightsCollectorUsersTests()
    {
        var databaseAbstraction = Mock.Of<IDatabaseAbstraction>(d =>
            d.GetTableName<User>() == UsersTable && d.GetTableName<Role>() == RolesTable &&
            d.GetTableName<Domain.DataTypes.DataType>() == DataTypesTable);

        _repo.Setup(r => r.DataTypeIdByTableName(UsersTable, It.IsAny<CancellationToken>())).ReturnsAsync(UsersDtId);
        _repo.Setup(r => r.DataTypeIdByTableName(RolesTable, It.IsAny<CancellationToken>())).ReturnsAsync(RolesDtId);
        _repo.Setup(r => r.DataTypeIdByTableName(DataTypesTable, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DataTypesDtId);
        //მიმდინარე მომხმარებლის როლების გასაღებები (UserMinLevel-ში ToListAsync-ს EF-ის მოთხოვნა სჭირდება)
        _repo.Setup(r => r.ManyToManyJoinsPc(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(_db.Context.ManyToManyJoins.Select(s => s.CKey));
        _repo.Setup(r => r.UserMinLevel(It.IsAny<IEnumerable<string>>())).Returns(ViewerLevel);
        _repo.Setup(r => r.UsersMinLevels(RolesDtId, UsersDtId, It.IsAny<CancellationToken>())).ReturnsAsync([
            new Tuple<int, int>(HigherRankedUserId, ViewerLevel - 5),
            new Tuple<int, int>(SameLevelUserId, ViewerLevel),
            new Tuple<int, int>(LowerRankedUserId, ViewerLevel + 10)
        ]);
        _repo.Setup(r => r.GetUsers(It.IsAny<CancellationToken>())).ReturnsAsync([
            new UserModel(RoleLessUserId, "NEWCOMER", "ახალი მომხმარებელი"),
            new UserModel(HigherRankedUserId, "BOSS", "უფროსი"),
            new UserModel(SameLevelUserId, "PEER", "თანატოლი"),
            new UserModel(LowerRankedUserId, "EDITOR", "რედაქტორი")
        ]);

        List<DataTypeModelForRvs> usersDataType =
            [new(UsersDtId, UsersTable, "მომხმარებლები", null, null, null, null, null, null)];
        _repo.Setup(r => r.ParentsDataTypesNormalView(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(usersDataType);
        _repo.Setup(r => r.ParentsDataTypesReverseView(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(usersDataType);
        _repo.Setup(r => r.ChildrenDataTypesNormalView(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(usersDataType);
        _repo.Setup(r => r.ChildrenDataTypesReverseView(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(usersDataType);

        _sut = new RightsCollector(_repo.Object, Mock.Of<IReturnValuesRepository>(), databaseAbstraction);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Theory]
    [InlineData(ERightsEditorViewStyle.NormalView)]
    [InlineData(ERightsEditorViewStyle.ReverseView)]
    public async Task ParentsTreeData_lists_role_less_users_and_hides_higher_ranked_ones(
        ERightsEditorViewStyle viewStyle)
    {
        Result<List<DataTypeModel>> result = await _sut.ParentsTreeData("admin", viewStyle);

        AssertListedUsers(result);
    }

    [Theory]
    [InlineData(ERightsEditorViewStyle.NormalView)]
    [InlineData(ERightsEditorViewStyle.ReverseView)]
    public async Task ChildrenTreeData_lists_role_less_users_and_hides_higher_ranked_ones(
        ERightsEditorViewStyle viewStyle)
    {
        Result<List<DataTypeModel>> result = await _sut.ChildrenTreeData("admin", RolesTable, viewStyle);

        AssertListedUsers(result);
    }

    private static void AssertListedUsers(Result<List<DataTypeModel>> result)
    {
        Assert.True(result.IsSuccess);
        DataTypeModel users = Assert.Single(result.Value);
        Assert.Equal(["NEWCOMER", "PEER", "EDITOR"], users.ReturnValues.Select(s => s.Key));
        Assert.DoesNotContain(users.ReturnValues, rv => rv.Id == HigherRankedUserId);
    }
}
