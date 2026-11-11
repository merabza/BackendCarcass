using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData;
using BackendCarcass.Application.MasterData.CellModels;
using BackendCarcass.Application.MasterData.Crud;
using BackendCarcass.Domain;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.MasterData.Crud;

public sealed class MasterDataCrudTests
{
    private const string TableName = "SortedThings";

    [Fact]
    public async Task GetAllRecords_SortedTableWithSortIdCell_ReturnsRecordsOrderedBySortId()
    {
        // Arrange
        MasterDataCrud crud = CreateCrud([new SortedThing(1, 3), new SortedThing(2, 1), new SortedThing(3, 2)]);

        // Act
        Result<IEnumerable<IDataType>> result = await crud.GetAllRecords();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal([2, 3, 1], result.Value.Select(x => x.Id));
    }

    private static MasterDataCrud CreateCrud(List<SortedThing> rows)
    {
        var entityType = new Mock<IEntityType>();
        entityType.Setup(x => x.ClrType).Returns(typeof(SortedThing));

        var cmdRepo = new Mock<ICarcassMasterDataRepository>();
        cmdRepo.Setup(x => x.GetEntityTypeByTableName(TableName)).Returns(entityType.Object);
        cmdRepo.Setup(x => x.SetMethodInfo()).Returns(typeof(object).GetMethod(nameof(ToString), Type.EmptyTypes));
        cmdRepo.Setup(x => x.RunGenericMethodForLoadAllRecords(It.IsAny<MethodInfo>(), It.IsAny<IReadOnlyTypeBase>()))
            .Returns(rows.AsQueryable());
        cmdRepo.Setup(x => x.GetDataTypeGridRulesByTableName(TableName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridModel { Cells = [IntegerCell.Create("sortId", "რიგითი ნომერი").SortId()] });

        Result<MasterDataCrud> createResult = MasterDataCrud.Create(TableName, NullLogger.Instance, cmdRepo.Object,
            new Mock<IUnitOfWork>().Object);
        Assert.True(createResult.IsSuccess);
        return createResult.Value;
    }

    private sealed class SortedThing(int id, int sortId) : ISortedDataType
    {
        public int Id { get; set; } = id;
        public int SortId { get; set; } = sortId;
        public string? Key => null;
        public string? Name => null;
        public int? ParentId => null;

        public bool UpdateTo(IDataType data)
        {
            return false;
        }

        public dynamic EditFields()
        {
            return this;
        }
    }
}
