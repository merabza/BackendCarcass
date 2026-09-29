using System;
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

    private static readonly JsonSerializerSettings CamelCaseSettings =
        new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    [Fact]
    public void Create_WithoutTypeName_SetsStringTypeAndGivenProperties()
    {
        // Act
        StringCell cell = StringCell.Create("name", "სახელი", false);

        // Assert
        Assert.Equal("String", cell.TypeName);
        Assert.Equal("name", cell.FieldName);
        Assert.Equal("სახელი", cell.Caption);
        Assert.False(cell.Visible);
    }

    [Fact]
    public void Create_WithoutVisibility_IsVisible()
    {
        // Act
        StringCell cell = StringCell.Create("name", "სახელი");

        // Assert
        Assert.True(cell.Visible);
    }

    [Fact]
    public void Create_WithTypeName_UsesGivenTypeName()
    {
        // Act
        StringCell cell = StringCell.Create("name", "სახელი", true, "Custom");

        // Assert
        Assert.Equal("Custom", cell.TypeName);
    }

    [Fact]
    public void Default_WithoutArgument_SetsEmptyDefault()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        StringCell result = cell.Default();

        // Assert
        Assert.Same(cell, result);
        Assert.Equal(string.Empty, cell.Def);
    }

    [Fact]
    public void Default_WithValue_SetsThatDefault()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        cell.Default("x");

        // Assert
        Assert.Equal("x", cell.Def);
    }

    [Fact]
    public void Required_WithoutArguments_SetsRequiredErrorBuiltFromCaption()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        StringCell result = cell.Required();

        // Assert
        Assert.Same(cell, result);
        Assert.NotNull(cell.IsRequiredErr);
        Assert.Equal("nameRequired", cell.IsRequiredErr.Code);
        Assert.Equal("სახელი შევსებული უნდა იყოს", cell.IsRequiredErr.Description);
    }

    [Fact]
    public void Required_WithCustomError_UsesCustomError()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        cell.Required("code0", "message0");

        // Assert
        Assert.NotNull(cell.IsRequiredErr);
        Assert.Equal("code0", cell.IsRequiredErr.Code);
        Assert.Equal("message0", cell.IsRequiredErr.Description);
    }

    [Fact]
    public void Max_Always_SetsMaxLenRuleWithDefaultError()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        StringCell result = cell.Max(255);

        // Assert
        Assert.Same(cell, result);
        Assert.NotNull(cell.MaxLenRule);
        Assert.Equal(255, cell.MaxLenRule.Val);
        Assert.Equal("nameTooLong", cell.MaxLenRule.Error.Code);
        Assert.Equal("სახელი ძალიან გრძელია", cell.MaxLenRule.Error.Description);
    }

    [Fact]
    public void Max_WithCustomError_UsesCustomError()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        cell.Max(255, "code3", "message3");

        // Assert
        Assert.NotNull(cell.MaxLenRule);
        Assert.Equal("code3", cell.MaxLenRule.Error.Code);
        Assert.Equal("message3", cell.MaxLenRule.Error.Description);
    }

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
    public void Nullable_Always_MarksCellNullable()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        StringCell result = cell.Nullable();

        // Assert
        Assert.Same(cell, result);
        Assert.True(cell.IsNullable);
    }

    [Fact]
    public void Validate_PlainString_ReturnsNoErrors()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი").Required().Max(10);

        // Act
        List<Error> errors = cell.Validate("ოთახი");

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NonStringValue_ReturnsTypeError()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        List<Error> errors = cell.Validate(5);

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("nameMustBeBoolean", error.Code);
        Assert.Equal("სახელი ველი უნდა იყოს სტრიქონის ტიპის", error.Description);
    }

    [Fact]
    public void Validate_NullOnNotNullableCell_ReturnsTypeError()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        List<Error> errors = cell.Validate(null);

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("nameMustBeBoolean", error.Code);
    }

    [Fact]
    public void Validate_NullOnRequiredCell_ReturnsRequiredError()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი").Required().Nullable();

        // Act
        List<Error> errors = cell.Validate(null);

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("nameRequired", error.Code);
    }

    [Fact]
    public void Validate_EmptyStringOnRequiredCell_ReturnsIsEmptyError()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი").Required();

        // Act
        List<Error> errors = cell.Validate(string.Empty);

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("nameIsEmpty", error.Code);
        Assert.Equal("სახელი შევსებული არ არის", error.Description);
    }

    [Fact]
    public void Validate_EmptyStringOnOptionalCell_ReturnsNoErrors()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        List<Error> errors = cell.Validate(string.Empty);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_LengthEqualToMax_ReturnsNoErrors()
    {
        // Arrange
        StringCell cell = StringCell.Create("code", "კოდი").Max(3);

        // Act
        List<Error> errors = cell.Validate("268");

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_LongerThanMax_ReturnsIsTooLongError()
    {
        // Arrange
        StringCell cell = StringCell.Create("code", "კოდი").Max(3);

        // Act
        List<Error> errors = cell.Validate("2680");

        // Assert
        Error error = Assert.Single(errors);
        Assert.Equal("codeIsTooLong", error.Code);
        Assert.Equal("კოდი ძალიან გრძელია", error.Description);
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
    public void Validate_EmptyStringOnRequiredCellWithMinAndPattern_ReturnsOnlyIsEmptyError()
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
    public void Validate_TooShortAndWrongFormat_ReturnsBothErrors()
    {
        // Arrange
        StringCell cell = StringCell.Create("personalId", "პირადი ნომერი").Min(11).Pattern(ElevenDigitsPattern);

        // Act
        List<Error> errors = cell.Validate("12a");

        // Assert
        Assert.Equal(["personalIdTooShort", "personalIdWrongFormat"], errors.ConvertAll(e => e.Code));
    }

    [Fact]
    public void Validate_TooLongAndWrongFormat_ReturnsBothErrors()
    {
        // Arrange
        StringCell cell = StringCell.Create("personalId", "პირადი ნომერი").Max(11).Pattern(ElevenDigitsPattern);

        // Act
        List<Error> errors = cell.Validate("1234567890123");

        // Assert
        Assert.Equal(["personalIdIsTooLong", "personalIdWrongFormat"], errors.ConvertAll(e => e.Code));
    }

    [Fact]
    public void Validate_PatternMatchingOnlyPartOfValue_ReturnsNoErrorsWithoutAnchors()
    {
        // Arrange
        StringCell cell = StringCell.Create("code", "კოდი").Pattern("[0-9]");

        // Act
        List<Error> errors = cell.Validate("a1b");

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void DeserializeGridModel_SerializedMinAndPattern_RestoresRules()
    {
        // Arrange
        var gridModel = new GridModel
        {
            Cells = [StringCell.Create("personalId", "პირადი ნომერი").Min(11, "c1", "m1").Pattern(ElevenDigitsPattern)]
        };
        string json = JsonConvert.SerializeObject(gridModel, CamelCaseSettings);

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
    public void Serialize_WithoutOptionalRules_OmitsThem()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი");

        // Act
        string json = JsonConvert.SerializeObject(cell, CamelCaseSettings);

        // Assert
        Assert.DoesNotContain("def", json, StringComparison.Ordinal);
        Assert.DoesNotContain("maxLenRule", json, StringComparison.Ordinal);
        Assert.DoesNotContain("minLenRule", json, StringComparison.Ordinal);
        Assert.DoesNotContain("patternRule", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialize_WithAllRules_WritesThem()
    {
        // Arrange
        StringCell cell = StringCell.Create("name", "სახელი").Default("x").Max(5).Min(2).Pattern("^a$");

        // Act
        string json = JsonConvert.SerializeObject(cell, CamelCaseSettings);

        // Assert
        Assert.Contains("\"def\":\"x\"", json, StringComparison.Ordinal);
        Assert.Contains("\"maxLenRule\":{\"val\":5", json, StringComparison.Ordinal);
        Assert.Contains("\"minLenRule\":{\"val\":2", json, StringComparison.Ordinal);
        Assert.Contains("\"patternRule\":{\"val\":\"^a$\"", json, StringComparison.Ordinal);
    }
}
