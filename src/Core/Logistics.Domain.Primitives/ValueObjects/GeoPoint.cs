using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
///     Represents a geographical point defined by its longitude and latitude coordinates.
/// </summary>
[ComplexType]
public record GeoPoint
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GeoPoint" /> class with specified longitude and latitude.
    ///     Longitude should be between -180 and 180, and latitude should be between -90 and 90.
    ///     Throws <see cref="ArgumentOutOfRangeException" /> if the values are out of range.
    /// </summary>
    /// <param name="longitude">Longitude coordinate of the point.</param>
    /// <param name="latitude">Latitude coordinate of the point.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when longitude is not in the range of -180 to 180 or latitude is
    ///     not in the range of -90 to 90.
    /// </exception>
    public GeoPoint(double longitude, double latitude)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(longitude, -180);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(longitude, 180);
        ArgumentOutOfRangeException.ThrowIfLessThan(latitude, -90);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(latitude, 90);

        Longitude = longitude;
        Latitude = latitude;
    }

    /// <summary>
    ///     Longitude coordinate of the point. Value should be between -180 and 180.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    ///     Latitude coordinate of the point. Value should be between -90 and 90.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    ///     Computes the great-circle distance to another point (metres) using
    ///     the Haversine formula.
    ///     Haversine formula is used to find the shortest distance between two points on the surface of a sphere.
    /// </summary>
    /// <param name="other">The other geographical point to which the distance is calculated.</param>
    /// <returns>Distance in metres.</returns>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public double DistanceTo(GeoPoint other)
    {
        const double R = 6_371_000; // Earth radius (m)
        var φ1 = DegreesToRadians(Latitude);
        var φ2 = DegreesToRadians(other.Latitude);
        var Δφ = φ2 - φ1;
        var Δλ = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Pow(Math.Sin(Δφ / 2), 2) +
                (Math.Cos(φ1) * Math.Cos(φ2) *
                 Math.Pow(Math.Sin(Δλ / 2), 2));

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    /// <summary>
    ///     Converts degrees to radians.
    /// </summary>
    private static double DegreesToRadians(double deg)
    {
        return deg * Math.PI / 180.0;
    }
}
