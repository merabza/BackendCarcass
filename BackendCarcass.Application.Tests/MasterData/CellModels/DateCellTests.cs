using System;
using System.Collections.Generic;
using BackendCarcass.Application.MasterData.CellModels;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.MasterData.CellModels;

public sealed class DateCellTests
{
    [Fact]
    public void Validate_TimeOnlyValue_ReturnsNoErrors()
    {
        // Arrange
        DateCell cell = DateCell.Create("lstTime", "დრო").Required().TimeOnly();

        // Act
        List<Error> errors = cell.Validate(new TimeOnly(8, 30));

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_TimeOnlyValueWithMinRule_ReturnsNoErrors()
    {
        // Arrange
        DateCell cell = DateCell.Create("lstTime", "დრო")
            .Min(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));

        // Act
        List<Error> errors = cell.Validate(new TimeOnly(8, 30));

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_DateTimeValue_ReturnsNoErrors()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი").Required();

        // Act
        List<Error> errors = cell.Validate(new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Unspecified));

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_DateTimeBeforeMin_ReturnsMinError()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი")
            .Min(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), "minCode", "minMessage");

        // Act
        List<Error> errors = cell.Validate(new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("minCode", error.Code);
    }

    [Fact]
    public void Validate_StringValue_ReturnsTypeError()
    {
        // Arrange
        DateCell cell = DateCell.Create("lstTime", "დრო").TimeOnly();

        // Act
        List<Error> errors = cell.Validate("08:30");

        // Assert
        Assert.Single(errors);
    }
}
