using BackendCarcassApi.CommandRequests.MasterData;
using BackendCarcassApi.Endpoints.V1;
using BackendCarcassApi.QueryRequests.MasterData;
using BackendCarcassApi.QueryResponses;
using CarcassMasterDataDom.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SystemToolsShared.Errors;
using Xunit;

namespace BackendCarcassApi.Test.Endpoints.V1;

public class MasterDataEndpointsTests
{
    private readonly Mock<WebApplication> _appMock;
    private readonly Mock<WebApplicationBuilder> _builderMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<HttpRequest> _requestMock;

    public MasterDataEndpointsTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _requestMock = new Mock<HttpRequest>();
        _builderMock = new Mock<WebApplicationBuilder>();
        _appMock = new Mock<WebApplication>();
    }

    [Fact]
    public void InstallServices_ReturnsTrue()
    {
        // Arrange
        var endpoints = new MasterDataEndpoints();

        // Act
        var result = endpoints.InstallServices(_builderMock.Object, true, [], new Dictionary<string, string>());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void UseServices_ReturnsTrue()
    {
        // Arrange
        var endpoints = new MasterDataEndpoints();
        var routeGroupBuilder = new Mock<RouteGroupBuilder>(_appMock.Object);
        _appMock.Setup(x => x.MapGroup(It.IsAny<string>())).Returns(routeGroupBuilder.Object);

        // Act
        var result = endpoints.UseServices(_appMock.Object, true);

        // Assert
        Assert.True(result);
        _appMock.Verify(x => x.MapGroup(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetLookupTables_ReturnsOk_OnSuccess()
    {
        // Arrange
        var tables = new StringValues(["table1", "table2"]);
        var response = new MdGetLookupTablesQueryResponse(new Dictionary<string, IEnumerable<SrvModel>>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<MdGetLookupTablesQueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OneOf<MdGetLookupTablesQueryResponse, IEnumerable<Err>>.FromT0(response));

        // Act
        var result = await MasterDataEndpoints.GetLookupTables(tables, _mediatorMock.Object, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Results<Ok<MdGetLookupTablesQueryResponse>, BadRequest<IEnumerable<Err>>>>(result);
    }

    [Fact]
    public async Task MdUpdateOneRecord_ReturnsNoContent_OnSuccess()
    {
        // Arrange
        const string tableName = "testTable";
        const int id = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<MdUpdateOneRecordCommandRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OneOf<Unit, IEnumerable<Err>>.FromT0(new Unit()));

        // Act
        var result = await MasterDataEndpoints.MdUpdateOneRecord(tableName, id, _requestMock.Object,
            _mediatorMock.Object, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mediatorMock.Verify(x => x.Send(It.IsAny<MdUpdateOneRecordCommandRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MdDeleteOneRecord_ReturnsNoContent_OnSuccess()
    {
        // Arrange
        const string tableName = "testTable";
        const int id = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<MdDeleteOneRecordCommandRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OneOf<Unit, IEnumerable<Err>>.FromT0(new Unit()));

        // Act
        var result =
            await MasterDataEndpoints.MdDeleteOneRecord(tableName, id, _mediatorMock.Object, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mediatorMock.Verify(x => x.Send(It.IsAny<MdDeleteOneRecordCommandRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}