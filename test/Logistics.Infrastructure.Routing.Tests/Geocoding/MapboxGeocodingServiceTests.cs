using System.Web;
using Logistics.Application.Abstractions.Tenancy;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Routing.Geocoding;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using MapboxOptions = Logistics.Infrastructure.Options.MapboxOptions;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace Logistics.Infrastructure.Routing.Tests.Geocoding;

public class MapboxGeocodingServiceTests
{
    private const string MapboxResponse =
        """
        {
          "type": "FeatureCollection",
          "features": [
            { "id": "addr.1", "place_name": "Test Place", "center": [10.0, 50.0], "relevance": 1.0 }
          ]
        }
        """;

    private readonly ICurrentTenantAccessor tenantAccessor = Substitute.For<ICurrentTenantAccessor>();

    private MapboxGeocodingService Build(CapturingHttpMessageHandler handler) =>
        new(
            new HttpClient(handler),
            MsOptions.Create(new MapboxOptions { AccessToken = "test-token" }),
            tenantAccessor,
            NullLogger<MapboxGeocodingService>.Instance);

    private static Tenant TenantWith(TenantSettings settings) => new()
    {
        Name = "Test Tenant",
        ConnectionString = "test",
        BillingEmail = "test@test.com",
        CompanyAddress = new Address
        {
            Line1 = "1 Main St",
            City = "City",
            State = "ST",
            ZipCode = "00000",
            Country = "US"
        },
        Settings = settings
    };

    [Fact]
    public async Task GeocodeAddressAsync_UsTenant_AddsCountryUsAndEnglishLanguage()
    {
        tenantAccessor.GetCurrentTenant().Returns(TenantWith(
            new TenantSettings { Region = Region.US, Language = "en" }));
        var handler = new CapturingHttpMessageHandler(MapboxResponse);
        var sut = Build(handler);

        var result = await sut.GeocodeAddressAsync("1 Main St", "Springfield", "IL", "62701");

        Assert.True(result.IsSuccess);
        var query = HttpUtility.ParseQueryString(handler.LastRequestUri!.Query);
        Assert.Equal("us", query["country"]);
        Assert.Equal("en", query["language"]);
    }

    [Fact]
    public async Task GeocodeAddressAsync_EuTenant_AddsCappedCountryListAndTenantLanguage()
    {
        tenantAccessor.GetCurrentTenant().Returns(TenantWith(
            new TenantSettings { Region = Region.EU, Language = "de" }));
        var handler = new CapturingHttpMessageHandler(MapboxResponse);
        var sut = Build(handler);

        var result = await sut.GeocodeAddressAsync("Unter den Linden 1", "Berlin", "BE", "10117");

        Assert.True(result.IsSuccess);
        var query = HttpUtility.ParseQueryString(handler.LastRequestUri!.Query);
        var countries = query["country"]!.Split(',');
        Assert.Equal(5, countries.Length);
        Assert.All(countries, c => Assert.Equal(c, c.ToLowerInvariant()));
        Assert.Equal("de", query["language"]);
    }

    [Fact]
    public async Task GeocodeAddressAsync_NoTenantContext_OmitsBiasButStillSucceeds()
    {
        tenantAccessor.GetCurrentTenant().Returns(_ => throw new InvalidOperationException("no tenant"));
        var handler = new CapturingHttpMessageHandler(MapboxResponse);
        var sut = Build(handler);

        var result = await sut.GeocodeAddressAsync("1 Main St", "Anywhere", "XX");

        Assert.True(result.IsSuccess);
        var query = HttpUtility.ParseQueryString(handler.LastRequestUri!.Query);
        Assert.Null(query["country"]);
        Assert.Null(query["language"]);
    }
}
