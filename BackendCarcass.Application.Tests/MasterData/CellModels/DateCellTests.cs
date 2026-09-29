using System;
using System.Collections.Generic;
using BackendCarcass.Application.MasterData.CellModels;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.MasterData.CellModels;

public sealed class DateCellTests
{
    private static readonly DateTime MinDate = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [Fact]
    public void Create_WithDefaults_ShowsDateAndTimeAndIsVisible()
    {
        // Act
        DateCell cell = DateCell.Create("startDate", "დასაწყისი");

        // Assert
        Assert.Equal("Date", cell.TypeName);
        Assert.Equal("startDate", cell.FieldName);
        Assert.Equal("დასაწყისი", cell.Caption);
        Assert.True(cell.ShowDate);
        Assert.True(cell.ShowTime);
        Assert.True(cell.Visible);
    }

    [Fact]
    public void Create_WithArguments_UsesThem()
    {
        // Act
        DateCell cell = DateCell.Create("startDate", "დასაწყისი", false, false, false, "Custom");

        // Assert
        Assert.Equal("Custom", cell.TypeName);
        Assert.False(cell.ShowDate);
        Assert.False(cell.ShowTime);
        Assert.False(cell.Visible);
    }

    [Fact]
    public void Required_WithoutArguments_SetsRequiredErrorBuiltFromCaption()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი");

        // Act
        DateCell result = cell.Required();

        // Assert
        Assert.Same(cell, result);
        Assert.NotNull(cell.IsRequiredErr);
        Assert.Equal("startDateRequired", cell.IsRequiredErr.Code);
        Assert.Equal("დასაწყისი შევსებული უნდა იყოს", cell.IsRequiredErr.Description);
    }

    [Fact]
    public void Required_WithCustomError_UsesCustomError()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი");

        // Act
        cell.Required("code0", "message0");

        // Assert
        Assert.NotNull(cell.IsRequiredErr);
        Assert.Equal("code0", cell.IsRequiredErr.Code);
        Assert.Equal("message0", cell.IsRequiredErr.Description);
    }

    [Fact]
    public void Nullable_Always_MarksCellNullable()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი");

        // Act
        DateCell result = cell.Nullable();

        // Assert
        Assert.Same(cell, result);
        Assert.True(cell.IsNullable);
    }

    [Fact]
    public void Min_WithoutError_SetsMinValRuleWithDefaultError()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი");

        // Act
        DateCell result = cell.Min(MinDate);

        // Assert
        Assert.Same(cell, result);
        Assert.NotNull(cell.MinValRule);
        Assert.Equal(MinDate, cell.MinValRule.Val);
        Assert.Equal($"startDateMinValueMustBe{MinDate}", cell.MinValRule.Error.Code);
        Assert.Equal($"დასაწყისი ველის მინიმალური დასაშვები მნიშვნელობაა {MinDate}",
            cell.MinValRule.Error.Description);
    }

    [Fact]
    public void Min_WithCustomError_UsesCustomError()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი");

        // Act
        cell.Min(MinDate, "minCode", "minMessage");

        // Assert
        Assert.NotNull(cell.MinValRule);
        Assert.Equal("minCode", cell.MinValRule.Error.Code);
        Assert.Equal("minMessage", cell.MinValRule.Error.Description);
    }

    [Fact]
    public void Default_WithValue_SetsThatDefault()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი");

        // Act
        DateCell result = cell.Default(MinDate);

        // Assert
        Assert.Same(cell, result);
        Assert.Equal(MinDate, cell.Def);
    }

    [Fact]
    public void Default_WithoutValue_SetsDefaultDateTime()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი");

        // Act
        cell.Default();

        // Assert
        Assert.Equal(default(DateTime), cell.Def);
    }

    [Fact]
    public void DateOnly_Always_ShowsOnlyTheDate()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი", false);

        // Act
        DateCell result = cell.DateOnly();

        // Assert
        Assert.Same(cell, result);
        Assert.True(cell.ShowDate);
        Assert.False(cell.ShowTime);
    }

    [Fact]
    public void TimeOnly_Always_ShowsOnlyTheTime()
    {
        // Arrange
        DateCell cell = DateCell.Create("lstTime", "დრო", true, false);

        // Act
        DateCell result = cell.TimeOnly();

        // Assert
        Assert.Same(cell, result);
        Assert.False(cell.ShowDate);
        Assert.True(cell.ShowTime);
    }

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
        DateCell cell = DateCell.Create("lstTime", "დრო").Min(MinDate);

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
        DateCell cell = DateCell.Create("startDate", "დასაწყისი").Min(MinDate, "minCode", "minMessage");

        // Act
        List<Error> errors = cell.Validate(MinDate.AddDays(-1));

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("minCode", error.Code);
    }

    [Fact]
    public void Validate_DateTimeEqualToMin_ReturnsNoErrors()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი").Min(MinDate);

        // Act
        List<Error> errors = cell.Validate(MinDate);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_StringValue_ReturnsTypeError()
    {
        // Arrange
        DateCell cell = DateCell.Create("lstTime", "დრო").TimeOnly();

        // Act
        List<Error> errors = cell.Validate("08:30");

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("lstTimeMustBeBoolean", error.Code);
        Assert.Equal("დრო ველი უნდა იყოს თარიღის ტიპის", error.Description);
    }

    [Fact]
    public void Validate_NullOnNullableCell_ReturnsNoErrors()
    {
        // Arrange
        DateCell cell = DateCell.Create("birthDate", "დაბადების თარიღი").Nullable().DateOnly();

        // Act
        List<Error> errors = cell.Validate(null);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullOnNotNullableCell_ReturnsTypeError()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი");

        // Act
        List<Error> errors = cell.Validate(null);

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("startDateMustBeBoolean", error.Code);
    }

    [Fact]
    public void Validate_NullOnRequiredNullableCell_ReturnsRequiredError()
    {
        // Arrange
        DateCell cell = DateCell.Create("startDate", "დასაწყისი").Required().Nullable();

        // Act
        List<Error> errors = cell.Validate(null);

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("startDateRequired", error.Code);
    }
}
