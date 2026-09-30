using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Rights;
using Microsoft.AspNetCore.Http;
using Moq;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Api.Tests.Filters;

// Shared mocks for the rights filters: the filters build a RightsDeterminer over these
internal sealed class RightsFilterTestContext
{
    public const int DataTypeId = 5;

    public RightsFilterTestContext(params string[] roles)
    {
        CurrentUser.Setup(u => u.Roles).Returns([.. roles]);
        DatabaseAbstraction.Setup(d => d.GetTableName<It.IsAnyType>()).Returns("AnyTable");
        Repository.Setup(r => r.GetDataTypeIdByKey(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DataTypeId);
    }

    public Mock<IUserRightsRepository> Repository { get; } = new();
    public Mock<ICurrentUser> CurrentUser { get; } = new();
    public Mock<IDatabaseAbstraction> DatabaseAbstraction { get; } = new();

    public int NextCalls { get; private set; }

    public static readonly object NextResult = new();

    public ValueTask<object?> Next(EndpointFilterInvocationContext context)
    {
        NextCalls++;
        return ValueTask.FromResult<object?>(NextResult);
    }

    public static EndpointFilterInvocationContext InvocationContext()
    {
        return new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), new List<object?>());
    }
}
