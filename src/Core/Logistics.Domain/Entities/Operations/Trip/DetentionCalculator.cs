namespace Logistics.Domain.Entities;

/// <summary>
/// Dwell at a stop is arrival to departure (or to now while the truck is still there). Detention is
/// the dwell past the free time, billed in whole quarter hours rounded down, so a customer is never
/// charged for minutes the carrier cannot show.
/// </summary>
public static class DetentionCalculator
{
    private const int BillingIncrementMinutes = 15;

    public static Detention Calculate(DateTime arrivedAt, DateTime? departedAt, DateTime now, DetentionTerms terms)
    {
        var end = departedAt ?? now;
        var dwellMinutes = Math.Max(0, (int)(end - arrivedAt).TotalMinutes);
        var overMinutes = Math.Max(0, dwellMinutes - terms.FreeMinutes);
        var billableHours = overMinutes / BillingIncrementMinutes * BillingIncrementMinutes / 60m;
        var charge = terms.RatePerHour is { } rate ? decimal.Round(billableHours * rate, 2) : 0m;

        return new Detention(dwellMinutes, terms.FreeMinutes, overMinutes, billableHours, terms.RatePerHour, charge,
            IsOngoing: departedAt is null);
    }

    /// <summary>Null when the truck has not arrived at the stop.</summary>
    public static Detention? Calculate(TripStop stop, DateTime now, DetentionTerms terms) =>
        stop.ArrivedAt is { } arrivedAt ? Calculate(arrivedAt, stop.DepartedAt, now, terms) : null;
}
