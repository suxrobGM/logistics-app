namespace Logistics.DbMigrator.Utils;

public static class MathUtils
{
    /// <summary>
    /// Calculates the great-circle distance between two points on a sphere using the Haversine formula.
    /// </summary>
    /// <param name="lat1">The latitude of the first point, in degrees.</param>
    /// <param name="lon1">The longitude of the first point, in degrees.</param>
    /// <param name="lat2">The latitude of the second point, in degrees.</param>
    /// <param name="lon2">The longitude of the second point, in degrees.</param>
    /// <returns>The distance between the two points, in meters.</returns>
    public static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6_371_000; // Earth radius in meters
        var dLat = (lat2 - lat1) * Math.PI / 180d;
        var dLon = (lon2 - lon1) * Math.PI / 180d;
        var a = Math.Sin(dLat/2)*Math.Sin(dLat/2) +
                   Math.Cos(lat1*Math.PI/180)*Math.Cos(lat2*Math.PI/180) *
                   Math.Sin(dLon/2)*Math.Sin(dLon/2);
        return 2 * r * Math.Asin(Math.Sqrt(a));
    }
}
