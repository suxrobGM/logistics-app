using Logistics.Domain.Entities;

namespace Logistics.Infrastructure.AI.Tools.Dispatch;

/// <summary>
/// Whether a driver's remaining Hours of Service can cover a trip. Shared by the single-shot and
/// batch tools so the two can never disagree - the reason strings reach the model verbatim and are
/// compared across tools when the agent evaluates candidates side by side.
/// </summary>
internal readonly record struct HosFeasibility(
    bool Feasible,
    bool FeasibleMultiDay,
    int EstimatedDrivingMinutes,
    string Reason)
{
    /// <summary>Average highway speed used to turn distance into driving minutes.</summary>
    private const double AverageSpeedKmh = 80.0;

    /// <summary>
    /// Below this, splitting the trip across rest breaks does not buy enough progress to be worth
    /// suggesting.
    /// </summary>
    private const int MinimumUsefulDrivingMinutes = 120;

    private const string NoData = "No HOS data available for this driver";

    public static int EstimateDrivingMinutes(double distanceKm) =>
        (int)(distanceKm / AverageSpeedKmh * 60);

    /// <summary>The verdict when the driver has no HOS row at all.</summary>
    public static HosFeasibility Unknown(double distanceKm) =>
        new(false, false, EstimateDrivingMinutes(distanceKm), NoData);

    public static HosFeasibility Evaluate(DriverHosStatus hos, double distanceKm)
    {
        var estimatedDrivingMinutes = EstimateDrivingMinutes(distanceKm);

        var feasible = !hos.IsInViolation
            && hos.DrivingMinutesRemaining >= estimatedDrivingMinutes
            && hos.OnDutyMinutesRemaining >= estimatedDrivingMinutes;

        var multiDay = !feasible
            && !hos.IsInViolation
            && hos.DrivingMinutesRemaining >= MinimumUsefulDrivingMinutes;

        var reason = $"Insufficient hours: need {estimatedDrivingMinutes}min, have {hos.DrivingMinutesRemaining}min driving - too low to make meaningful progress";
        if (feasible)
        {
            reason = "Driver has sufficient hours to complete in one stretch";
        }
        else if (hos.IsInViolation)
        {
            reason = "Driver is currently in HOS violation";
        }
        else if (multiDay)
        {
            reason = $"Not completable in current window (need {estimatedDrivingMinutes}min, have {hos.DrivingMinutesRemaining}min), but feasible as multi-day trip with rest stops";
        }

        return new HosFeasibility(feasible, multiDay, estimatedDrivingMinutes, reason);
    }
}
