using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
/// Represents the route information for a load, including origin and destination.
/// </summary>
[ComplexType]
public record LoadRoute
{
    public LoadRoute(
        Address originAddress,
        GeoPoint originLocation,
        Address destinationAddress,
        GeoPoint destinationLocation)
    {
        OriginAddress = originAddress;
        OriginLocation = originLocation;
        DestinationAddress = destinationAddress;
        DestinationLocation = destinationLocation;
        Distance = CalculateDistance();
    }

    /// <summary>
    /// Required for EF Core.
    /// </summary>
    private LoadRoute()
    {
        OriginAddress = null!;
        OriginLocation = null!;
        DestinationAddress = null!;
        DestinationLocation = null!;
    }

    public Address OriginAddress { get; private set; }
    public GeoPoint OriginLocation { get; private set; }

    public Address DestinationAddress { get; private set; }
    public GeoPoint DestinationLocation { get; private set; }

    /// <summary>
    /// Total distance in kilometers.
    /// </summary>
    public double Distance { get; private set; }

    /// <summary>
    /// Updates the origin address and location, recalculating distance.
    /// </summary>
    public LoadRoute WithOrigin(Address address, GeoPoint location)
    {
        return new LoadRoute(address, location, DestinationAddress, DestinationLocation);
    }

    /// <summary>
    /// Updates the destination address and location, recalculating distance.
    /// </summary>
    public LoadRoute WithDestination(Address address, GeoPoint location)
    {
        return new LoadRoute(OriginAddress, OriginLocation, address, location);
    }

    /// <summary>
    /// Calculates the distance between origin and destination in kilometers.
    /// </summary>
    private double CalculateDistance()
    {
        return OriginLocation.DistanceTo(DestinationLocation) / 1000.0;
    }
}
