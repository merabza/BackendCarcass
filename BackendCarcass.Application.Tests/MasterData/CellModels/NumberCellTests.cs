using System;
using System.Collections.Generic;
using BackendCarcass.Application.MasterData.CellModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.MasterData.CellModels;

public sealed class NumberCellTests
{
    [Fact]
    public void Create_WithoutTypeName_SetsNumberTypeAndGivenProperties()
    {
        // Act
        NumberCell cell = NumberCell.Create("hourSalaryNet", "საათის ხელფასი", false);

        // Assert
        Assert.Equal("Number", cell.TypeName);
        Assert.Equal("hourSalaryNet", cell.FieldName);
        Assert.Equal("საათის ხელფასი", cell.Caption);
        Assert.False(cell.Visible);
        Assert.Null(cell.IsPositiveErr);
    }

    [Fact]
    public void Create_WithoutVisibility_IsVisible()
    {
        // Act
        NumberCell cell = NumberCell.Create("hourSalaryNet", "საათის ხელფასი");

        // Assert
        Assert.True(cell.Visible);
    }

    [Fact]
    public void Create_WithTypeName_UsesGivenTypeName()
    {
        // Act
        NumberCell cell = NumberCell.Create("hourSalaryNet", "საათის ხელფასი", true, "Custom");

        // Assert
        Assert.Equal("Custom", cell.TypeName);
    }

    [Fact]
    public void Required_WithoutArguments_SetsRequiredErrorBuiltFromCaption()
    {
        // Arrange
        NumberCell cell = NumberCell.Create("hourSalaryNet", "საათის ხელფასი");

        // Act
        NumberCell result = cell.Required();

        // Assert
        Assert.Same(cell, result);
        Assert.NotNull(cell.IsRequiredErr);
        Assert.Equal("hourSalaryNetRequired", cell.IsRequiredErr.Code);
        Assert.Equal("საათის ხელფასი შევსებული უნდა იყოს", cell.IsRequiredErr.Description);
    }

    [Fact]
    public void Required_WithCustomError_UsesCustomError()
    {
        // Arrange
        NumberCell cell = NumberCell.Create("hourSalaryNet", "საათის ხელფასი");

        // Act
        cell.Required("code0", "message0");

        // Assert
        Assert.NotNull(cell.IsRequiredErr);
        Assert.Equal("code0", cell.IsRequiredErr.Code);
        Assert.Equal("message0", cell.IsRequiredErr.Description);
    }

    [Fact]
    public void Positive_WithoutArguments_SetsPositiveErrorBuiltFromCaption()
    {
        // Arrange
        var cell = new ExposedNumberCell("hourSalaryNet", "საათის ხელფასი");

        // Act
        NumberCell result = cell.CallPositive();

        // Assert
        Assert.Same(cell, result);
        Assert.NotNull(cell.IsPositiveErr);
        Assert.Equal("hourSalaryNetMustBePositive", cell.IsPositiveErr.Code);
        Assert.Equal("საათის ხელფასი უნდა იყოს დადებითი რიცხვი", cell.IsPositiveErr.Description);
    }

    [Fact]
    public void Positive_WithCustomError_UsesCustomError()
    {
        // Arrange
        var cell = new ExposedNumberCell("hourSalaryNet", "საათის ხელფასი");

        // Act
        cell.CallPositive("code1", "message1");

        // Assert
        Assert.NotNull(cell.IsPositiveErr);
        Assert.Equal("code1", cell.IsPositiveErr.Code);
        Assert.Equal("message1", cell.IsPositiveErr.Description);
    }

    [Fact]
    public void Nullable_Always_MarksCellNullable()
    {
        // Arrange
        var cell = new ExposedNumberCell("hourSalaryNet", "საათის ხელფასი");

        // Act
        NumberCell result = cell.CallNullable();

        // Assert
        Assert.Same(cell, result);
        Assert.True(cell.IsNullable);
    }

    [Fact]
    public void Validate_NullOnRequiredCell_ReturnsRequiredError()
    {
        // Arrange
        NumberCell cell = NumberCell.Create("hourSalaryNet", "საათის ხელფასი").Required();

        // Act
        List<Error> errors = cell.Validate(null);

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("hourSalaryNetRequired", error.Code);
    }

    [Fact]
    public void Validate_DecimalOnRequiredCell_ReturnsNoErrors()
    {
        // Arrange
        NumberCell cell = NumberCell.Create("hourSalaryNet", "საათის ხელფასი").Required();

        // Act
        List<Error> errors = cell.Validate(5.25m);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Serialize_WithRequiredError_WritesNumberTypeAndOmitsPositiveError()
    {
        // Arrange
        NumberCell cell = NumberCell.Create("hourSalaryNet", "საათის ხელფასი").Required();

        // Act
        string json = JsonConvert.SerializeObject(cell,
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

        // Assert
        Assert.Contains("\"typeName\":\"Number\"", json, StringComparison.Ordinal);
        Assert.Contains("\"isRequiredErr\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("isPositiveErr", json, StringComparison.Ordinal);
    }

    // exposes the protected members the carcass keeps for derived cells (IntegerCell)
    private sealed class ExposedNumberCell(string fieldName, string? caption) : NumberCell(fieldName, caption)
    {
        public NumberCell CallPositive(string? errorCode = null, string? errorMessage = null)
        {
            return Positive(errorCode, errorMessage);
        }

        public NumberCell CallNullable()
        {
            return Nullable();
        }
    }
}
