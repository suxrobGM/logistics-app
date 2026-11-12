using Microsoft.Extensions.Logging.Abstractions;

namespace Logistics.Infrastructure.Vin.Tests;

public class WmiPrefixDecoderTests
{
    private const string ManGermanyResponse =
        """
        {
          "Count": 1,
          "Results": [
            {
              "ManufacturerName": "MAN TRUCK & BUS",
              "CommonName": "MAN",
              "Name": "MAN Truck & Bus",
              "CountryName": "GERMANY",
              "VehicleType": "Truck"
            }
          ]
        }
        """;

    private const string EmptyResponse = """{ "Count": 0, "Results": [] }""";

    private static WmiPrefixDecoder Build(StubHttpMessageHandler handler)
    {
        var client = new HttpClient(handler);
        return new WmiPrefixDecoder(client, NullLogger<WmiPrefixDecoder>.Instance);
    }

    [Fact]
    public async Task DecodeVinAsync_ManGermanyPrefix_ReturnsMakeAndCountry()
    {
        var sut = Build(StubHttpMessageHandler.Json(ManGermanyResponse));

        var result = await sut.DecodeVinAsync("WMA06XZZ8KM123456");

        Assert.NotNull(result);
        Assert.Equal("MAN", result!.Make);
        Assert.Equal("Germany", result.CountryOfManufacture);
        Assert.Equal("Truck", result.VehicleType);
        Assert.Equal("wmi", result.Source);
        Assert.Null(result.Model);
        Assert.Null(result.Year);
    }

    [Fact]
    public async Task DecodeVinAsync_EmptyResults_ReturnsNull()
    {
        var sut = Build(StubHttpMessageHandler.Json(EmptyResponse));

        var result = await sut.DecodeVinAsync("ZZZ00000000000000");

        Assert.Null(result);
    }

    [Fact]
    public async Task DecodeVinAsync_HttpFailure_ReturnsNull()
    {
        var sut = Build(StubHttpMessageHandler.Throws(new HttpRequestException("network down")));

        var result = await sut.DecodeVinAsync("WMA06XZZ8KM123456");

        Assert.Null(result);
    }

    [Fact]
    public async Task DecodeVinAsync_InvalidVinLength_ReturnsNull()
    {
        var sut = Build(StubHttpMessageHandler.Json(ManGermanyResponse));

        var result = await sut.DecodeVinAsync("SHORT");

        Assert.Null(result);
    }
}
