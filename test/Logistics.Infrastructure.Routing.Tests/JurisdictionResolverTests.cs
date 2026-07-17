using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Routing.Geospatial;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Logistics.Infrastructure.Routing.Tests;

public class JurisdictionResolverTests
{
    private static readonly JurisdictionResolver sut = new(NullLogger<JurisdictionResolver>.Instance);

    [Theory]
    [InlineData(-96.797, 32.7767, "US", "TX")] // Dallas
    [InlineData(-87.6298, 41.8781, "US", "IL")] // Chicago
    [InlineData(-112.074, 33.4484, "US", "AZ")] // Phoenix
    [InlineData(-79.3832, 43.6532, "CA", "ON")] // Toronto
    [InlineData(-113.4938, 53.5461, "CA", "AB")] // Edmonton
    public void Resolve_KnownCity_ReturnsExpectedJurisdiction(
        double longitude, double latitude, string expectedCountry, string expectedRegion)
    {
        var jurisdiction = sut.Resolve(new GeoPoint(longitude, latitude));

        Assert.NotNull(jurisdiction);
        Assert.Equal(expectedCountry, jurisdiction.CountryCode);
        Assert.Equal(expectedRegion, jurisdiction.Region);
    }

    [Fact]
    public void Resolve_MidAtlantic_ReturnsNull()
    {
        var jurisdiction = sut.Resolve(new GeoPoint(-40.0, 35.0));

        Assert.Null(jurisdiction);
    }

    [Fact]
    public void Resolve_BorderCity_ReturnsOneSide()
    {
        // Texarkana sits on the TX/AR border; simplified polygons must still pick exactly one side
        var jurisdiction = sut.Resolve(new GeoPoint(-94.0477, 33.4418));

        Assert.NotNull(jurisdiction);
        Assert.Equal("US", jurisdiction.CountryCode);
        Assert.Contains(jurisdiction.Region, new[] { "TX", "AR" });
    }
}
