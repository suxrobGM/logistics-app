namespace Logistics.DriverApp.Helpers;

public static class DistanceConverter
{
    public static double FromMeterTo(double value, DistanceUnit unit)
    {
        return unit switch
        {
            DistanceUnit.Meter => value,
            DistanceUnit.Mile => value * 0.000621371,
            DistanceUnit.Kilometer => value * 0.001,
            DistanceUnit.Yard => value * 1.09361,
            _ => value,
        };
    }
}

public enum DistanceUnit
{
    Meter,
    Mile,
    Kilometer,
    Yard,
}
