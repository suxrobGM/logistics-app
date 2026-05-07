using FluentValidation.TestHelper;
using Logistics.Application.Validators;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Validators;

public class AddressValidatorTests
{
    private readonly AddressValidator sut = new();

    private static Address ValidAddress() => new()
    {
        Line1 = "1600 Amphitheatre Pkwy",
        City = "Mountain View",
        State = "CA",
        ZipCode = "94043",
        Country = "US",
    };

    [Fact]
    public void Validate_FullyPopulated_Passes()
    {
        var result = sut.TestValidate(ValidAddress());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("US", "CA")]
    [InlineData("CA", "ON")]
    [InlineData("AU", "NSW")]
    [InlineData("DE", "Bayern")]
    [InlineData("NL", "Noord-Holland")]
    [InlineData("BE", "Antwerpen")]
    [InlineData("IE", "Dublin")]
    [InlineData("GB", "Greater London")]
    [InlineData("FR", "Île-de-France")]
    [InlineData("MX", "Jalisco")]
    public void Validate_StateProvidedForAnyCountry_Passes(string country, string state)
    {
        var address = ValidAddress() with { Country = country, State = state };
        var result = sut.TestValidate(address);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("US")]
    [InlineData("DE")]
    [InlineData("NL")]
    public void Validate_StateMissing_FailsForAllCountries(string country)
    {
        var address = ValidAddress() with { Country = country, State = "" };
        var result = sut.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.State);
    }

    [Fact]
    public void Validate_Line1Empty_Fails()
    {
        var address = ValidAddress() with { Line1 = "" };
        var result = sut.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.Line1);
    }

    [Fact]
    public void Validate_CityEmpty_Fails()
    {
        var address = ValidAddress() with { City = "" };
        var result = sut.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.City);
    }

    [Fact]
    public void Validate_ZipCodeEmpty_Fails()
    {
        var address = ValidAddress() with { ZipCode = "" };
        var result = sut.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.ZipCode);
    }

    [Fact]
    public void Validate_CountryNot2Letters_Fails()
    {
        var address = ValidAddress() with { Country = "USA" };
        var result = sut.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.Country);
    }

    [Fact]
    public void Validate_Line1OverMaxLength_Fails()
    {
        var address = ValidAddress() with { Line1 = new string('x', 201) };
        var result = sut.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.Line1);
    }
}
