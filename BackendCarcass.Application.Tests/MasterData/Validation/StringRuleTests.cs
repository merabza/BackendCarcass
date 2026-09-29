using BackendCarcass.Application.MasterData.Validation;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.MasterData.Validation;

public sealed class StringRuleTests
{
    [Fact]
    public void Constructor_Always_KeepsValueAndBuildsProblemError()
    {
        // Act
        var rule = new StringRule("^[0-9]{9}$", "phoneNumberWrongFormat", "ტელეფონი არასწორი ფორმატისაა");

        // Assert
        Assert.Equal("^[0-9]{9}$", rule.Val);
        Assert.Equal("phoneNumberWrongFormat", rule.Error.Code);
        Assert.Equal("ტელეფონი არასწორი ფორმატისაა", rule.Error.Description);
        Assert.Equal(ErrorType.Problem, rule.Error.Type);
    }
}
