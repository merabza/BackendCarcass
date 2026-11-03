using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.MasterData.Models;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace BackendCarcass.Application.Tests.Identity;

// The validator adds the rules Identity's own PasswordOptions cannot express: at least one letter, nothing from the
// user's own identifiers and nothing from the common-passwords list. Length, digits and unique characters stay with
// Identity, so a password that only breaks those rules passes here
public sealed class CarcassPasswordValidatorTests
{
    private readonly AppUser _user = new("merab", "Merab", "Test") { Email = "merab.test@example.com" };
    private readonly CarcassPasswordValidator _validator = new();

    [Theory]
    [InlineData("Correct-Horse-Battery-9")]
    [InlineData("ქართული პაროლი 2026")]
    [InlineData("short1")]
    public async Task Accepts_passwords_that_break_none_of_its_rules(string password)
    {
        IdentityResult result = await _validator.ValidateAsync(null!, _user, password);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Rejects_a_password_without_a_letter()
    {
        IdentityResult result = await _validator.ValidateAsync(null!, _user, "1234567890-+=!");

        Assert.Equal(["PasswordRequiresLetter"], result.Errors.Select(e => e.Code));
    }

    [Theory]
    [InlineData("xxMERABxx2026")]
    [InlineData("Merab.Test-2026")]
    public async Task Rejects_a_password_containing_the_user_name_or_the_email_local_part(string password)
    {
        IdentityResult result = await _validator.ValidateAsync(null!, _user, password);

        Assert.Equal(["PasswordContainsUserName"], result.Errors.Select(e => e.Code));
    }

    [Fact]
    public async Task Ignores_identifiers_shorter_than_three_characters()
    {
        var user = new AppUser("ab", "A", "B") { Email = "ab@example.com" };

        IdentityResult result = await _validator.ValidateAsync(null!, user, "abracadabra-2026");

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("password1234")]
    [InlineData("QwertyUIOP123")]
    [InlineData(" iloveyou123 ")]
    public async Task Rejects_common_passwords_regardless_of_case_and_surrounding_spaces(string password)
    {
        IdentityResult result = await _validator.ValidateAsync(null!, _user, password);

        Assert.Equal(["PasswordIsTooCommon"], result.Errors.Select(e => e.Code));
    }

    [Fact]
    public async Task Reports_every_broken_rule_at_once()
    {
        var user = new AppUser("123456", "A", "B") { Email = "x@example.com" };

        IdentityResult result = await _validator.ValidateAsync(null!, user, "123456");

        Assert.Equal(["PasswordRequiresLetter", "PasswordContainsUserName", "PasswordIsTooCommon"],
            result.Errors.Select(e => e.Code));
    }

    [Fact]
    public async Task Treats_a_null_password_as_empty()
    {
        IdentityResult result = await _validator.ValidateAsync(null!, _user, null);

        Assert.Equal(["PasswordRequiresLetter"], result.Errors.Select(e => e.Code));
    }
}
