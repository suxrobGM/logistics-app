using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.FuelCards.Common;
using Logistics.Infrastructure.Integrations.FuelCards.Providers.Efs;
using Logistics.Infrastructure.Integrations.FuelCards.Providers.Wex;
using Xunit;

namespace Logistics.Application.Tests.FuelCards;

public class FuelCardProviderMapperTests
{
    #region WEX

    [Fact]
    public void WexMapper_FullTransaction_MapsAllFields()
    {
        var wex = new WexTransaction
        {
            TransactionId = "W-1",
            TransactionDate = new DateTime(2026, 7, 1, 14, 30, 0),
            Amount = 412.50m,
            Quantity = 110m,
            UnitOfMeasure = "GAL",
            PricePerUnit = 3.75m,
            ProductDescription = "Diesel",
            MerchantName = "Pilot",
            MerchantCity = "Dallas",
            MerchantState = "TX",
            MerchantCountry = "US",
            CardNumber = "****1234",
            CardId = "CARD-9",
            VehicleNumber = "T-100",
            DriverName = "Jane Doe"
        };

        var data = WexMapper.ToTransactionData(wex);

        Assert.NotNull(data);
        Assert.Equal("W-1", data.ExternalTransactionId);
        Assert.Equal(412.50m, data.Amount);
        Assert.Equal(VolumeUnit.Gallons, data.QuantityUnit);
        Assert.Equal("US", data.PurchaseJurisdiction!.CountryCode);
        Assert.Equal("TX", data.PurchaseJurisdiction.Region);
        Assert.Equal("CARD-9", data.ExternalCardId);
        Assert.Equal("T-100", data.UnitNumber);
        Assert.Equal(DateTimeKind.Utc, data.TransactionDate.Kind);
        Assert.NotNull(data.RawJson);
    }

    [Theory]
    [InlineData(null, "2026-07-01", 100)]
    [InlineData("W-2", null, 100)]
    [InlineData("W-3", "2026-07-01", null)]
    public void WexMapper_MissingRequiredField_ReturnsNull(string? id, string? date, int? amount)
    {
        var wex = new WexTransaction
        {
            TransactionId = id,
            TransactionDate = date is null ? null : DateTime.Parse(date),
            Amount = amount
        };

        Assert.Null(WexMapper.ToTransactionData(wex));
    }

    #endregion

    #region EFS

    [Fact]
    public void EfsMapper_FullTransaction_MapsAllFields()
    {
        var efs = new EfsTransaction
        {
            Id = "E-1",
            PostedDate = new DateTime(2026, 7, 2),
            TotalAmount = 300m,
            Gallons = 80m,
            UnitPrice = 3.60m,
            ProductCode = "ULSD",
            LocationName = "Love's",
            LocationCity = "Toronto",
            LocationState = "ON",
            CardNumber = "****5678",
            UnitNumber = "T-200"
        };

        var data = EfsMapper.ToTransactionData(efs);

        Assert.NotNull(data);
        Assert.Equal("E-1", data.ExternalTransactionId);
        Assert.Equal(VolumeUnit.Gallons, data.QuantityUnit);
        // Canadian province code without an explicit country is inferred as CA
        Assert.Equal("CA", data.PurchaseJurisdiction!.CountryCode);
        Assert.Equal("ON", data.PurchaseJurisdiction.Region);
        Assert.Equal("****5678", data.ExternalCardId);
    }

    #endregion

    #region Jurisdiction inference

    [Theory]
    [InlineData(null, "TX", "US", "TX")]
    [InlineData(null, "ON", "CA", "ON")]
    [InlineData("USA", "ca", "US", "CA")] // California with explicit USA country, not Canada
    [InlineData("CAN", "QC", "CA", "QC")]
    [InlineData("MEX", "NL", "MX", "NL")]
    public void JurisdictionMapper_InfersCountry(string? country, string? state, string expectedCountry, string expectedRegion)
    {
        var jurisdiction = JurisdictionMapper.FromMerchant(country, state);

        Assert.NotNull(jurisdiction);
        Assert.Equal(expectedCountry, jurisdiction.CountryCode);
        Assert.Equal(expectedRegion, jurisdiction.Region);
    }

    [Fact]
    public void JurisdictionMapper_NothingProvided_ReturnsNull()
    {
        Assert.Null(JurisdictionMapper.FromMerchant(null, null));
        Assert.Null(JurisdictionMapper.FromMerchant("", " "));
    }

    #endregion
}
