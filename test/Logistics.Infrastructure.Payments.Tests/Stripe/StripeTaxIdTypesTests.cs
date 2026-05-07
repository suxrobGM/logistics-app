using Logistics.Infrastructure.Payments.Stripe;

namespace Logistics.Infrastructure.Payments.Tests.Stripe;

public class StripeTaxIdTypesTests
{
    [Theory]
    [InlineData("GB", "GB123456789", "gb_vat")]
    [InlineData("US", "12-3456789", "us_ein")]
    [InlineData("AU", "12345678901", "au_abn")]
    [InlineData("CA", "123456789RT0001", "ca_gst_hst")]
    [InlineData("JP", "1234567890123", "jp_cn")]
    [InlineData("IN", "27ABCDE1234F1Z5", "in_gst")]
    [InlineData("MX", "ABCD123456EF7", "mx_rfc")]
    [InlineData("BR", "12.345.678/0001-90", "br_cnpj")]
    [InlineData("ZA", "4123456789", "za_vat")]
    [InlineData("CH", "CHE-123.456.789", "ch_vat")]
    [InlineData("NO", "123456789MVA", "no_vat")]
    [InlineData("NZ", "123456789", "nz_gst")]
    public void Infer_DirectCountryMapping_ReturnsExpected(string country, string taxId, string expected)
    {
        Assert.Equal(expected, StripeTaxIdTypes.Infer(country, taxId));
    }

    [Theory]
    [InlineData("DE", "DE123456789", "eu_vat")]
    [InlineData("FR", "FR12345678901", "eu_vat")]
    [InlineData("NL", "NL123456789B01", "eu_vat")]
    public void Infer_EuMembers_ReturnEuVat(string country, string taxId, string expected)
    {
        Assert.Equal(expected, StripeTaxIdTypes.Infer(country, taxId));
    }

    [Theory]
    [InlineData("XX", "CHE-123", "ch_vat")]   // CH prefix wins when country unknown
    [InlineData("XX", "NO123456789", "no_vat")]
    public void Infer_UnknownCountry_FallsBackOnVatIdPrefix(string country, string taxId, string expected)
    {
        Assert.Equal(expected, StripeTaxIdTypes.Infer(country, taxId));
    }

    [Fact]
    public void Infer_UnknownCountryAndPrefix_DefaultsToEuVat()
    {
        Assert.Equal("eu_vat", StripeTaxIdTypes.Infer("XX", "ABC123"));
    }

    [Theory]
    [InlineData("gb")]   // case-insensitive
    [InlineData("Gb")]
    [InlineData("GB")]
    public void Infer_CountryCodeCaseInsensitive(string country)
    {
        Assert.Equal("gb_vat", StripeTaxIdTypes.Infer(country, "GB123"));
    }
}
