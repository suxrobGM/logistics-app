using FluentValidation.TestHelper;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;
using Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

namespace Logistics.Application.Tests.Validators;

public class UpdateTenantValidatorTests
{
    private readonly UpdateTenantValidator sut = new();

    private static UpdateTenantCommand BaseCommand() => new()
    {
        Id = Guid.NewGuid(),
        Name = "acme",
        BillingEmail = "billing@acme.test",
    };

    #region VAT number

    [Theory]
    [InlineData("DE123456789")]
    [InlineData("GB987654321")]
    [InlineData("FRAB123456789")]
    [InlineData("IE1234567X")]
    public void Validate_ValidVatNumber_Passes(string vat)
    {
        var cmd = BaseCommand();
        cmd.VatNumber = vat;
        var result = sut.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(c => c.VatNumber);
    }

    [Theory]
    [InlineData("DE12")]            // too short
    [InlineData("123456789")]       // missing country prefix
    [InlineData("de123456789")]     // lowercase prefix
    [InlineData("DE-123-456")]      // dashes not allowed
    public void Validate_InvalidVatNumber_Fails(string vat)
    {
        var cmd = BaseCommand();
        cmd.VatNumber = vat;
        var result = sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(c => c.VatNumber);
    }

    [Fact]
    public void Validate_VatNumberNullOrEmpty_Skipped()
    {
        var cmd = BaseCommand();
        cmd.VatNumber = null;
        var result = sut.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(c => c.VatNumber);

        cmd.VatNumber = "";
        result = sut.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(c => c.VatNumber);
    }

    #endregion

    #region MC number

    [Theory]
    [InlineData("MC1234567")]
    [InlineData("MC-1234567")]
    [InlineData("1234567")]
    [InlineData("9999")]
    public void Validate_ValidMcNumber_Passes(string mc)
    {
        var cmd = BaseCommand();
        cmd.McNumber = mc;
        var result = sut.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(c => c.McNumber);
    }

    [Theory]
    [InlineData("MCabc")]       // letters
    [InlineData("123")]          // too short
    [InlineData("123456789")]    // too long
    [InlineData("MC 1234")]      // space
    public void Validate_InvalidMcNumber_Fails(string mc)
    {
        var cmd = BaseCommand();
        cmd.McNumber = mc;
        var result = sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(c => c.McNumber);
    }

    #endregion

    #region EORI number

    [Theory]
    [InlineData("DE1234567890")]
    [InlineData("GB123456789000")]
    [InlineData("NLABC123")]
    public void Validate_ValidEoriNumber_Passes(string eori)
    {
        var cmd = BaseCommand();
        cmd.EoriNumber = eori;
        var result = sut.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(c => c.EoriNumber);
    }

    [Theory]
    [InlineData("DE")]                          // missing body
    [InlineData("12DE3456")]                    // digits before letters
    [InlineData("DE1234567890123456789")]       // exceeds max length
    public void Validate_InvalidEoriNumber_Fails(string eori)
    {
        var cmd = BaseCommand();
        cmd.EoriNumber = eori;
        var result = sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(c => c.EoriNumber);
    }

    #endregion

    #region Tax residency

    [Theory]
    [InlineData("US")]
    [InlineData("DE")]
    public void Validate_TaxResidency2Letters_Passes(string code)
    {
        var cmd = BaseCommand();
        cmd.TaxResidencyCountry = code;
        var result = sut.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(c => c.TaxResidencyCountry);
    }

    [Theory]
    [InlineData("USA")]
    [InlineData("U")]
    public void Validate_TaxResidencyWrongLength_Fails(string code)
    {
        var cmd = BaseCommand();
        cmd.TaxResidencyCountry = code;
        var result = sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(c => c.TaxResidencyCountry);
    }

    #endregion

    #region Company address

    [Fact]
    public void Validate_ValidCompanyAddress_Passes()
    {
        var cmd = BaseCommand();
        cmd.CompanyAddress = new Address
        {
            Line1 = "1 Main St",
            City = "Berlin",
            State = "Berlin",
            ZipCode = "10115",
            Country = "DE",
        };
        var result = sut.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(c => c.CompanyAddress);
    }

    [Fact]
    public void Validate_CompanyAddressMissingState_Fails()
    {
        var cmd = BaseCommand();
        cmd.CompanyAddress = new Address
        {
            Line1 = "1 Main St",
            City = "Berlin",
            State = "",
            ZipCode = "10115",
            Country = "DE",
        };
        var result = sut.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor("CompanyAddress.State");
    }

    #endregion
}
