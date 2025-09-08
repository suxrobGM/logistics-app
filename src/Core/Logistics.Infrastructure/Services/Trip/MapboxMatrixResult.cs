namespace Logistics.Infrastructure.Services.Trip;

public sealed record MapboxMatrixResult(double?[][] Durations, double?[][] Distances)
{
    public double Cost(int from, int to)
    {
        return Distances[from][to] ?? double.PositiveInfinity;
    }

    public double Time(int from, int to)
    {
        return Durations[from][to] ?? double.PositiveInfinity;
    }
}
