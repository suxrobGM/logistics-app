using Microsoft.Extensions.Logging.Abstractions;

namespace Logistics.Infrastructure.Vin.Tests;

public class CompositeVinDecoderServiceTests
{
    private const string ManWmiResponse =
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

    private const string EmptyWmiResponse = """{ "Count": 0, "Results": [] }""";

    private const string FullNhtsaResponse =
        """
        {
          "Count": 1,
          "Results": [
            {
              "ErrorCode": "0",
              "Make": "FREIGHTLINER",
              "Model": "Cascadia",
              "ModelYear": "2023",
              "BodyClass": "Truck-Tractor",
              "VehicleType": "TRUCK",
              "DriveType": "6x4",
              "FuelTypePrimary": "Diesel",
              "DisplacementL": "12.7",
              "EngineCylinders": "6",
              "EngineHP": "455"
            }
          ]
        }
        """;

    private const string EmptyNhtsaResponse = """{ "Count": 0, "Results": [] }""";

    private static CompositeVinDecoderService Build(string wmiBody, string nhtsaBody)
    {
        var wmi = new WmiPrefixDecoder(
            new HttpClient(StubHttpMessageHandler.Json(wmiBody)),
            NullLogger<WmiPrefixDecoder>.Instance);

        var nhtsa = new NhtsaVinDecoderService(
            new HttpClient(StubHttpMessageHandler.Json(nhtsaBody)),
            NullLogger<NhtsaVinDecoderService>.Instance);

        return new CompositeVinDecoderService(wmi, nhtsa, NullLogger<CompositeVinDecoderService>.Instance);
    }

    [Fact]
    public async Task DecodeVinAsync_BothDecodersReturnData_MergesWithWmiPlusNhtsaSource()
    {
        var sut = Build(ManWmiResponse, FullNhtsaResponse);

        var result = await sut.DecodeVinAsync("1FUJBBCK57LX12345");

        Assert.NotNull(result);
        Assert.Equal("FREIGHTLINER", result!.Make);
        Assert.Equal("Cascadia", result.Model);
        Assert.Equal(2023, result.Year);
        Assert.Equal("Germany", result.CountryOfManufacture);
        Assert.Equal("wmi+nhtsa", result.Source);
    }

    [Fact]
    public async Task DecodeVinAsync_NhtsaEmpty_FallsBackToWmiOnly()
    {
        var sut = Build(ManWmiResponse, EmptyNhtsaResponse);

        var result = await sut.DecodeVinAsync("WMA06XZZ8KM123456");

        Assert.NotNull(result);
        Assert.Equal("MAN", result!.Make);
        Assert.Equal("Germany", result.CountryOfManufacture);
        Assert.Equal("wmi", result.Source);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task DecodeVinAsync_WmiEmpty_ReturnsNhtsaOnly()
    {
        var sut = Build(EmptyWmiResponse, FullNhtsaResponse);

        var result = await sut.DecodeVinAsync("1FUJBBCK57LX12345");

        Assert.NotNull(result);
        Assert.Equal("FREIGHTLINER", result!.Make);
        Assert.Equal("nhtsa", result.Source);
        Assert.Null(result.CountryOfManufacture);
    }

    [Fact]
    public async Task DecodeVinAsync_BothEmpty_ReturnsNull()
    {
        var sut = Build(EmptyWmiResponse, EmptyNhtsaResponse);

        var result = await sut.DecodeVinAsync("ZZZZZZZZZZZZZZZZZ");

        Assert.Null(result);
    }

    [Fact]
    public async Task DecodeVinAsync_InvalidVinLength_ReturnsNullWithoutCallingDecoders()
    {
        var calls = 0;
        var counting = new StubHttpMessageHandler(_ =>
        {
            calls++;
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(ManWmiResponse, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var wmi = new WmiPrefixDecoder(new HttpClient(counting), NullLogger<WmiPrefixDecoder>.Instance);
        var nhtsa = new NhtsaVinDecoderService(new HttpClient(counting), NullLogger<NhtsaVinDecoderService>.Instance);
        var sut = new CompositeVinDecoderService(wmi, nhtsa, NullLogger<CompositeVinDecoderService>.Instance);

        var result = await sut.DecodeVinAsync("TOOSHORT");

        Assert.Null(result);
        Assert.Equal(0, calls);
    }
}
