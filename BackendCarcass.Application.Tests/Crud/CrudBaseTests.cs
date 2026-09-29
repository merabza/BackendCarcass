using System;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Crud;
using BackendCarcass.Application.Crud.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;
using Xunit;

namespace BackendCarcass.Application.Tests.Crud;

public sealed class CrudBaseTests
{
    private const string DuplicateKeyMessage =
        "Cannot insert duplicate key row in object 'dbo.Humans' with unique index 'IX_Humans_PersonalId'.";

    [Fact]
    public async Task Update_SaveThrowsDuplicateKey_ReturnsSuchARecordAlreadyExists()
    {
        // Arrange
        var crud = new TestCrud(new ThrowingUnitOfWork(new InvalidOperationException("outer",
            new InvalidOperationException(DuplicateKeyMessage))));

        // Act
        Result result = await crud.Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SystemToolsErrors.SuchARecordAlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task Update_SaveThrowsOtherInnerException_ReturnsUnexpectedApiException()
    {
        // Arrange
        var crud = new TestCrud(new ThrowingUnitOfWork(new InvalidOperationException("outer",
            new InvalidOperationException("Some other database error"))));

        // Act
        Result result = await crud.Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("UnexpectedApiException", result.Error.Code);
    }

    [Fact]
    public async Task Update_SaveThrowsWithoutInnerException_ReturnsUnexpectedApiException()
    {
        // Arrange
        var crud = new TestCrud(new ThrowingUnitOfWork(new InvalidOperationException(DuplicateKeyMessage)));

        // Act
        Result result = await crud.Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("UnexpectedApiException", result.Error.Code);
    }

    [Fact]
    public async Task Update_SaveSucceeds_ReturnsSuccess()
    {
        // Arrange
        var crud = new TestCrud(new ThrowingUnitOfWork(null));

        // Act
        Result result = await crud.Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsSuccess);
    }

    private sealed class TestCrudData : ICrudData;

    private sealed class ThrowingUnitOfWork(Exception? exception) : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return exception is null ? Task.CompletedTask : Task.FromException(exception);
        }

        public string GetTableName<T>() where T : class
        {
            return typeof(T).Name;
        }

        public IEntityType? GetEntityTypeByTableName(string tableName)
        {
            return null;
        }

        public void SetCommandTimeout(TimeSpan timeout)
        {
        }
    }

    private sealed class TestCrud(IUnitOfWork unitOfWork) : CrudBase(NullLogger.Instance, unitOfWork)
    {
        public override ValueTask<Result<TableRowsData>> GetTableRowsData(FilterSortRequest filterSortRequest,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        protected override Task<Result<ICrudData>> GetOneData(int id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        protected override ValueTask<Result> CreateData(ICrudData crudDataForCreate,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        protected override ValueTask<Result> UpdateData(int id, ICrudData crudDataNewVersion,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(Result.Success());
        }

        protected override Task<Result> DeleteData(int id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
