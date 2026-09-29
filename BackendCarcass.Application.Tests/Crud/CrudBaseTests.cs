using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Crud;
using BackendCarcass.Application.Crud.Models;
using Microsoft.Extensions.Logging;
using Moq;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;
using Xunit;

namespace BackendCarcass.Application.Tests.Crud;

// CrudBase wraps the data operations of the master-data pages: it saves the unit of work and turns the database
// errors the users can cause (a duplicate unique key, a row still referenced by a foreign key) into controlled errors
public sealed class CrudBaseTests
{
    private const string UnexpectedApiExceptionCode = "UnexpectedApiException";

    private const string DuplicateKeyMessage =
        "Cannot insert duplicate key row in object 'dbo.Humans' with unique index 'IX_Humans_PersonalId'.";

    private const string ReferenceConflictMessage =
        "The DELETE statement conflicted with the REFERENCE constraint \"FK_Groups_Courses_CourseId\".";

    private readonly Mock<ILogger> _logger = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public CrudBaseTests()
    {
        _logger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
    }

    [Fact]
    public async Task GetOne_DataLoaded_ReturnsTheLoadedData()
    {
        // Arrange
        var data = new TestCrudData();
        TestCrud crud = CreateCrud();
        crud.OnGetOneData = _ => Task.FromResult(Result.Success<ICrudData>(data));

        // Act
        Result<ICrudData> result = await crud.GetOne(5);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(data, result.Value);
        Assert.Equal([5], crud.LoadedIds);
    }

    [Fact]
    public void GetOne_LoadingThrows_LogsTheErrorAndRethrows()
    {
        // Arrange
        var exception = new InvalidOperationException("load failed");
        TestCrud crud = CreateCrud();
        crud.OnGetOneData = _ => throw exception;

        // Act
        var thrown = Assert.Throws<InvalidOperationException>(() => { _ = crud.GetOne(5); });

        // Assert
        Assert.Same(exception, thrown);
        VerifyErrorLogged(exception, "GetOne", Times.Once());
    }

    [Fact]
    public void GetOne_LoadingThrowsWhileErrorLoggingIsOff_RethrowsWithoutLogging()
    {
        // Arrange
        TurnErrorLoggingOff();
        TestCrud crud = CreateCrud();
        crud.OnGetOneData = _ => throw new InvalidOperationException("load failed");

        // Act
        Assert.Throws<InvalidOperationException>(() => { _ = crud.GetOne(5); });

        // Assert
        VerifyNothingLogged();
    }

    [Fact]
    public async Task Create_DataCreated_SavesAndReturnsTheCreatedRecord()
    {
        // Arrange
        var data = new TestCrudData();
        var crud = new TestCrudWithCreatedId(_logger.Object, _unitOfWork.Object)
        {
            OnGetOneData = _ => Task.FromResult(Result.Success<ICrudData>(data))
        };

        // Act
        Result<ICrudData> result = await crud.Create(new TestCrudData());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(data, result.Value);
        Assert.Equal([TestCrudWithCreatedId.CreatedId], crud.LoadedIds);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithoutJustCreatedIdOverride_LoadsRecordZero()
    {
        // Arrange
        TestCrud crud = CreateCrud();
        crud.OnGetOneData = _ => Task.FromResult(Result.Success<ICrudData>(new TestCrudData()));

        // Act
        await crud.Create(new TestCrudData());

        // Assert
        Assert.Equal([0], crud.LoadedIds);
    }

    [Fact]
    public async Task Create_CreateDataFails_ReturnsItsErrorWithoutSaving()
    {
        // Arrange
        Error error = Error.Problem("createFailed", "ჩანაწერი ვერ შეიქმნა");
        TestCrud crud = CreateCrud();
        crud.OnCreateData = () => ValueTask.FromResult(Result.Failure(error));

        // Act
        Result<ICrudData> result = await crud.Create(new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Empty(crud.LoadedIds);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_SaveThrowsDuplicateKey_LogsTheCauseAndReturnsSuchARecordAlreadyExists()
    {
        // Arrange
        var cause = new InvalidOperationException(DuplicateKeyMessage);
        SaveThrows(new InvalidOperationException("outer", cause));

        // Act
        Result<ICrudData> result = await CreateCrud().Create(new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SystemToolsErrors.SuchARecordAlreadyExists.Code, result.Error.Code);
        VerifyErrorLogged(cause, "Create", Times.Once());
        VerifyErrorsLogged(Times.Once());
    }

    [Fact]
    public async Task Create_SaveThrowsOtherInnerError_LogsBothAndReturnsUnexpectedApiException()
    {
        // Arrange
        var cause = new InvalidOperationException("Some other database error");
        var exception = new InvalidOperationException("outer", cause);
        SaveThrows(exception);

        // Act
        Result<ICrudData> result = await CreateCrud().Create(new TestCrudData());

        // Assert
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
        VerifyErrorLogged(cause, "Create", Times.Once());
        VerifyErrorLogged(exception, "Create", Times.Once());
    }

    [Fact]
    public async Task Create_SaveThrowsDuplicateKeyWithoutInnerError_LogsItAndReturnsUnexpectedApiException()
    {
        // Arrange
        var exception = new InvalidOperationException(DuplicateKeyMessage);
        SaveThrows(exception);

        // Act
        Result<ICrudData> result = await CreateCrud().Create(new TestCrudData());

        // Assert
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
        VerifyErrorLogged(exception, "Create", Times.Once());
        VerifyErrorsLogged(Times.Once());
    }

    [Fact]
    public async Task Create_SaveThrowsWhileErrorLoggingIsOff_ReturnsTheErrorWithoutLogging()
    {
        // Arrange
        TurnErrorLoggingOff();
        SaveThrows(new InvalidOperationException("outer", new InvalidOperationException("other")));

        // Act
        Result<ICrudData> result = await CreateCrud().Create(new TestCrudData());

        // Assert
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
        VerifyNothingLogged();
    }

    [Fact]
    public async Task Create_SaveThrowsDuplicateKeyWhileErrorLoggingIsOff_ReturnsSuchARecordAlreadyExists()
    {
        // Arrange
        TurnErrorLoggingOff();
        SaveThrows(new InvalidOperationException("outer", new InvalidOperationException(DuplicateKeyMessage)));

        // Act
        Result<ICrudData> result = await CreateCrud().Create(new TestCrudData());

        // Assert
        Assert.Equal(SystemToolsErrors.SuchARecordAlreadyExists.Code, result.Error.Code);
        VerifyNothingLogged();
    }

    [Fact]
    public async Task Create_LoggingTheErrorFails_LogsThatFailureAndReturnsUnexpectedApiException()
    {
        // Arrange
        var loggingFailure = new InvalidOperationException("log is full");
        FirstErrorLogThrows(loggingFailure);
        SaveThrows(new InvalidOperationException("outer", new InvalidOperationException("other")));

        // Act
        Result<ICrudData> result = await CreateCrud().Create(new TestCrudData());

        // Assert
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
        VerifyErrorLogged(loggingFailure, "Create", Times.Once());
    }

    [Fact]
    public async Task Create_ErrorHandlingFailsWhileErrorLoggingIsOff_ReturnsUnexpectedApiExceptionWithoutLogging()
    {
        // Arrange
        TurnErrorLoggingOff();
        SaveThrows(new InvalidOperationException("outer", new BrokenMessageException()));

        // Act
        Result<ICrudData> result = await CreateCrud().Create(new TestCrudData());

        // Assert
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
        VerifyNothingLogged();
    }

    [Fact]
    public async Task Update_Succeeds_SavesTwiceAndReturnsSuccess()
    {
        // Arrange
        TestCrud crud = CreateCrud();

        // Act
        Result result = await crud.Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Update_UpdateDataFails_ReturnsItsErrorWithoutSaving()
    {
        // Arrange
        Error error = Error.Problem("updateFailed", "ჩანაწერი ვერ შეიცვალა");
        TestCrud crud = CreateCrud();
        crud.OnUpdateData = () => ValueTask.FromResult(Result.Failure(error));

        // Act
        Result result = await crud.Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_AfterUpdateDataFails_ReturnsItsErrorAfterTheFirstSave()
    {
        // Arrange
        Error error = Error.Problem("reSortFailed", "დალაგება ვერ შეიცვალა");
        var crud = new TestCrudWithAfterUpdate(_logger.Object, _unitOfWork.Object, Result.Failure(error));

        // Act
        Result result = await crud.Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_AfterUpdateDataSucceeds_SavesItsChangesToo()
    {
        // Arrange
        var crud = new TestCrudWithAfterUpdate(_logger.Object, _unitOfWork.Object, Result.Success());

        // Act
        Result result = await crud.Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Update_SaveThrowsDuplicateKey_ReturnsSuchARecordAlreadyExists()
    {
        // Arrange
        SaveThrows(new InvalidOperationException("outer", new InvalidOperationException(DuplicateKeyMessage)));

        // Act
        Result result = await CreateCrud().Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SystemToolsErrors.SuchARecordAlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task Update_SaveThrowsOtherInnerError_ReturnsUnexpectedApiException()
    {
        // Arrange
        SaveThrows(new InvalidOperationException("outer", new InvalidOperationException("Some other database error")));

        // Act
        Result result = await CreateCrud().Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
    }

    [Fact]
    public async Task Update_SaveThrowsDuplicateKeyWithoutInnerError_ReturnsUnexpectedApiException()
    {
        // Arrange
        SaveThrows(new InvalidOperationException(DuplicateKeyMessage));

        // Act
        Result result = await CreateCrud().Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
    }

    [Fact]
    public async Task Update_ErrorHandlingFails_ReturnsUnexpectedApiException()
    {
        // Arrange
        SaveThrows(new InvalidOperationException("outer", new BrokenMessageException()));

        // Act
        Result result = await CreateCrud().Update(1, new TestCrudData());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
    }

    [Fact]
    public async Task Delete_Succeeds_SavesAndReturnsSuccess()
    {
        // Arrange
        TestCrud crud = CreateCrud();

        // Act
        Result result = await crud.Delete(3);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal([3], crud.DeletedIds);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeleteDataFails_ReturnsItsErrorWithoutSaving()
    {
        // Arrange
        Error error = Error.Problem("deleteFailed", "ჩანაწერი ვერ წაიშალა");
        TestCrud crud = CreateCrud();
        crud.OnDeleteData = () => Task.FromResult(Result.Failure(error));

        // Act
        Result result = await crud.Delete(3);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_SaveThrowsReferenceConflict_LogsTheCauseAndReturnsTheEntryHasBeenUsed()
    {
        // Arrange
        var cause = new InvalidOperationException(ReferenceConflictMessage);
        SaveThrows(new InvalidOperationException("outer", cause));

        // Act
        Result result = await CreateCrud().Delete(3);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SystemToolsErrors.TheEntryHasBeenUsedAndCannotBeDeleted.Code, result.Error.Code);
        VerifyErrorLogged(cause, "Delete", Times.Once());
        VerifyErrorsLogged(Times.Once());
    }

    [Fact]
    public async Task Delete_SaveThrowsOtherInnerError_LogsBothAndReturnsUnexpectedApiException()
    {
        // Arrange
        var cause = new InvalidOperationException("Some other database error");
        var exception = new InvalidOperationException("outer", cause);
        SaveThrows(exception);

        // Act
        Result result = await CreateCrud().Delete(3);

        // Assert
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
        VerifyErrorLogged(cause, "Delete", Times.Once());
        VerifyErrorLogged(exception, "Delete", Times.Once());
    }

    [Fact]
    public async Task Delete_SaveThrowsReferenceConflictWithoutInnerError_LogsItAndReturnsUnexpectedApiException()
    {
        // Arrange
        var exception = new InvalidOperationException(ReferenceConflictMessage);
        SaveThrows(exception);

        // Act
        Result result = await CreateCrud().Delete(3);

        // Assert
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
        VerifyErrorLogged(exception, "Delete", Times.Once());
        VerifyErrorsLogged(Times.Once());
    }

    [Fact]
    public async Task Delete_SaveThrowsWhileErrorLoggingIsOff_ReturnsTheErrorWithoutLogging()
    {
        // Arrange
        TurnErrorLoggingOff();
        SaveThrows(new InvalidOperationException("outer", new InvalidOperationException(ReferenceConflictMessage)));

        // Act
        Result result = await CreateCrud().Delete(3);

        // Assert
        Assert.Equal(SystemToolsErrors.TheEntryHasBeenUsedAndCannotBeDeleted.Code, result.Error.Code);
        VerifyNothingLogged();
    }

    [Fact]
    public async Task Delete_SaveThrowsOtherErrorWhileErrorLoggingIsOff_ReturnsUnexpectedApiExceptionWithoutLogging()
    {
        // Arrange
        TurnErrorLoggingOff();
        SaveThrows(new InvalidOperationException("outer", new InvalidOperationException("other")));

        // Act
        Result result = await CreateCrud().Delete(3);

        // Assert
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
        VerifyNothingLogged();
    }

    [Fact]
    public async Task Delete_LoggingTheErrorFails_ReturnsUnexpectedApiException()
    {
        // Arrange
        FirstErrorLogThrows(new InvalidOperationException("log is full"));
        SaveThrows(new InvalidOperationException("outer", new InvalidOperationException(ReferenceConflictMessage)));

        // Act
        Result result = await CreateCrud().Delete(3);

        // Assert
        Assert.Equal(UnexpectedApiExceptionCode, result.Error.Code);
    }

    private TestCrud CreateCrud()
    {
        return new TestCrud(_logger.Object, _unitOfWork.Object);
    }

    private void SaveThrows(Exception exception)
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);
    }

    private void TurnErrorLoggingOff()
    {
        _logger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(false);
    }

    private void FirstErrorLogThrows(Exception loggingFailure)
    {
        int calls = 0;
        _logger.Setup(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(new InvocationAction(_ =>
            {
                calls++;
                if (calls == 1)
                {
                    throw loggingFailure;
                }
            }));
    }

    private void VerifyErrorLogged(Exception exception, string methodName, Times times)
    {
        string message = $"Error occurred executing {methodName}.";
        _logger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString() == message), exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);
    }

    private void VerifyErrorsLogged(Times times)
    {
        _logger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);
    }

    // CrudBase logs errors only
    private void VerifyNothingLogged()
    {
        VerifyErrorsLogged(Times.Never());
    }

    // an error whose text cannot be read, so handling it fails inside the catch block
    public sealed class BrokenMessageException : Exception
    {
        public override string Message => throw new InvalidOperationException("the message is not available");
    }

    private sealed class TestCrudData : ICrudData;

    private class TestCrud(ILogger logger, IUnitOfWork unitOfWork) : CrudBase(logger, unitOfWork)
    {
        public Func<int, Task<Result<ICrudData>>> OnGetOneData { get; set; } =
            _ => Task.FromResult(Result.Success<ICrudData>(new TestCrudData()));

        public Func<ValueTask<Result>> OnCreateData { get; set; } = () => ValueTask.FromResult(Result.Success());

        public Func<ValueTask<Result>> OnUpdateData { get; set; } = () => ValueTask.FromResult(Result.Success());

        public Func<Task<Result>> OnDeleteData { get; set; } = () => Task.FromResult(Result.Success());

        public List<int> LoadedIds { get; } = [];

        public List<int> DeletedIds { get; } = [];

        public override ValueTask<Result<TableRowsData>> GetTableRowsData(FilterSortRequest filterSortRequest,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        protected override Task<Result<ICrudData>> GetOneData(int id, CancellationToken cancellationToken = default)
        {
            LoadedIds.Add(id);
            return OnGetOneData(id);
        }

        protected override ValueTask<Result> CreateData(ICrudData crudDataForCreate,
            CancellationToken cancellationToken = default)
        {
            return OnCreateData();
        }

        protected override ValueTask<Result> UpdateData(int id, ICrudData crudDataNewVersion,
            CancellationToken cancellationToken = default)
        {
            return OnUpdateData();
        }

        protected override Task<Result> DeleteData(int id, CancellationToken cancellationToken = default)
        {
            DeletedIds.Add(id);
            return OnDeleteData();
        }
    }

    private sealed class TestCrudWithCreatedId(ILogger logger, IUnitOfWork unitOfWork) : TestCrud(logger, unitOfWork)
    {
        public const int CreatedId = 7;

        protected override int JustCreatedId => CreatedId;
    }

    private sealed class TestCrudWithAfterUpdate(ILogger logger, IUnitOfWork unitOfWork, Result afterUpdateResult)
        : TestCrud(logger, unitOfWork)
    {
        protected override ValueTask<Result> AfterUpdateData(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(afterUpdateResult);
        }
    }
}
