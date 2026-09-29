using System.Collections.Generic;
using BackendCarcass.Application.MasterData;
using BackendCarcass.Application.MasterData.CellModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.MasterData.CellModels;

public sealed class StringCellTests
{
    private const string ElevenDigitsPattern = "^[0-9]{11}$";

    [Fact]
    public void Min_Always_SetsMinLenRuleWithDefaultError()
    {
        // Arrange
        StringCell cell = StringCell.Create("personalId", "პირადი ნომერი");

        // Act
        StringCell result = cell.Min(11);

        // Assert
        Assert.Same(cell, result);
        Assert.NotNull(cell.MinLenRule);
        Assert.Equal(11, cell.MinLenRule.Val);
        Assert.Equal("personalIdTooShort", cell.MinLenRule.Error.Code);
        Assert.Equal("პირადი ნომერი ძალიან მოკლეა", cell.MinLenRule.Error.Description);
    }

    [Fact]
    public void Min_WithCustomError_UsesCustomError()
    {
        // Arrange
        StringCell cell = StringCell.Create("personalId", "პირადი ნომერი");

        // Act
        cell.Min(11, "code1", "message1");

        // Assert
        Assert.NotNull(cell.MinLenRule);
        Assert.Equal("code1", cell.MinLenRule.Error.Code);
        Assert.Equal("message1", cell.MinLenRule.Error.Description);
    }

    [Fact]
    public void Pattern_Always_SetsPatternRuleWithDefaultError()
    {
        // Arrange
        StringCell cell = StringCell.Create("phoneNumber", "ტელეფონი");

        // Act
        StringCell result = cell.Pattern("^[0-9]{9}$");

        // Assert
        Assert.Same(cell, result);
        Assert.NotNull(cell.PatternRule);
        Assert.Equal("^[0-9]{9}$", cell.PatternRule.Val);
        Assert.Equal("phoneNumberWrongFormat", cell.PatternRule.Error.Code);
        Assert.Equal("ტელეფონი არასწორი ფორმატისაა", cell.PatternRule.Error.Description);
    }

    [Fact]
    public void Pattern_WithCustomError_UsesCustomError()
    {
        // Arrange
        StringCell cell = StringCell.Create("phoneNumber", "ტელეფონი");

        // Act
        cell.Pattern("^[0-9]{9}$", "code2", "message2");

        // Assert
        Assert.NotNull(cell.PatternRule);
        Assert.Equal("code2", cell.PatternRule.Error.Code);
        Assert.Equal("message2", cell.PatternRule.Error.Description);
    }

    [Fact]
    public void Validate_ShorterThanMin_ReturnsMinLenError()
    {
        // Arrange
        StringCell cell = StringCell.Create("personalId", "პირადი ნომერი").Min(11);

        // Act
        List<Error> errors = cell.Validate("1234567890");

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("personalIdTooShort", error.Code);
    }

    [Fact]
    public void Validate_ExactlyMinLength_ReturnsNoErrors()
    {
        // Arrange
        StringCell cell = StringCell.Create("personalId", "პირადი ნომერი").Min(11);

        // Act
        List<Error> errors = cell.Validate("12345678901");

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_MatchingPattern_ReturnsNoErrors()
    {
        // Arrange
        StringCell cell = StringCell.Create("personalId", "პირადი ნომერი").Pattern(ElevenDigitsPattern);

        // Act
        List<Error> errors = cell.Validate("01234567890");

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("0123456789a")]
    [InlineData("123456789012")]
    [InlineData("1234")]
    public void Validate_NotMatchingPattern_ReturnsPatternError(string value)
    {
        // Arrange
        StringCell cell = StringCell.Create("personalId", "პირადი ნომერი").Pattern(ElevenDigitsPattern);

        // Act
        List<Error> errors = cell.Validate(value);

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("personalIdWrongFormat", error.Code);
    }

    [Fact]
    public void Validate_EmptyStringWithMinAndPattern_ReturnsNoErrors()
    {
        // Arrange
        StringCell cell = StringCell.Create("phoneNumber", "ტელეფონი").Nullable().Min(9).Pattern("^[0-9]{9}$");

        // Act
        List<Error> errors = cell.Validate(string.Empty);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullOnNullableCellWithMinAndPattern_ReturnsNoErrors()
    {
        // Arrange
        StringCell cell = StringCell.Create("phoneNumber", "ტელეფონი").Nullable().Min(9).Pattern("^[0-9]{9}$");

        // Act
        List<Error> errors = cell.Validate(null);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EmptyStringOnRequiredCell_ReturnsOnlyIsEmptyError()
    {
        // Arrange
        StringCell cell = StringCell.Create("personalId", "პირადი ნომერი").Required().Min(11)
            .Pattern(ElevenDigitsPattern);

        // Act
        List<Error> errors = cell.Validate(string.Empty);

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("personalIdIsEmpty", error.Code);
    }

    [Fact]
    public void Validate_TooLongAndWrongFormat_ReturnsBothErrors()
    {
        // Arrange
        StringCell cell = StringCell.Create("personalId", "პირადი ნომერი").Max(11).Pattern(ElevenDigitsPattern);

        // Act
        List<Error> errors = cell.Validate("1234567890123");

        // Assert
        Assert.Equal(2, errors.Count);
        Assert.Contains(errors, e => e.Code == "personalIdIsTooLong");
        Assert.Contains(errors, e => e.Code == "personalIdWrongFormat");
    }

    [Fact]
    public void DeserializeGridModel_SerializedMinAndPattern_RestoresRules()
    {
        // Arrange
        var gridModel = new GridModel
        {
            Cells = [StringCell.Create("personalId", "პირადი ნომერი").Min(11, "c1", "m1").Pattern(ElevenDigitsPattern)]
        };
        string json = JsonConvert.SerializeObject(gridModel,
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

        // Act
        GridModel? restored = GridModel.DeserializeGridModel(json);

        // Assert
        Assert.NotNull(restored);
        StringCell cell = Assert.IsType<StringCell>(Assert.Single(restored.Cells));
        Assert.NotNull(cell.MinLenRule);
        Assert.Equal(11, cell.MinLenRule.Val);
        Assert.Equal("c1", cell.MinLenRule.Error.Code);
        Assert.Equal("m1", cell.MinLenRule.Error.Description);
        Assert.NotNull(cell.PatternRule);
        Assert.Equal(ElevenDigitsPattern, cell.PatternRule.Val);
        Assert.Equal("personalIdWrongFormat", cell.PatternRule.Error.Code);
    }

    [Fact]
    public void Serialize_WithoutMinAndPattern_OmitsRules()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი").Max(255);

        // Act
        string json = JsonConvert.SerializeObject(cell,
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

        // Assert
        Assert.DoesNotContain("minLenRule", json, System.StringComparison.Ordinal);
        Assert.DoesNotContain("patternRule", json, System.StringComparison.Ordinal);
    }
}
