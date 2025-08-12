using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Utilities;

public static class SubscriptionUtils
{
    /// <summary>
    /// Gets the trial end date based on the trial period.
    /// </summary>
    /// <param name="period">The trial period.</param>
    /// <returns>The DateTime representing the trial end date. If the period is None, returns null.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the trial period is not recognized.</exception>
    public static DateTime? GetTrialEndDate(TrialPeriod period)
    {
        return period switch
        {
            TrialPeriod.None => null,
            TrialPeriod.SevenDays => DateTime.UtcNow.AddDays(7),
            TrialPeriod.FourteenDays => DateTime.UtcNow.AddDays(14),
            TrialPeriod.ThirtyDays => DateTime.UtcNow.AddDays(30),
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
        };
    }

    /// <summary>
    /// Gets the number of trial days based on the trial period.
    /// </summary>
    /// <param name="period">The trial period.</param>
    /// <returns>The number of trial days.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the trial period is not recognized.</exception>
    public static int GetTrialDays(TrialPeriod period)
    {
        return period switch
        {
            TrialPeriod.None => 0,
            TrialPeriod.SevenDays => 7,
            TrialPeriod.FourteenDays => 14,
            TrialPeriod.ThirtyDays => 30,
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
        };
    }

    /// <summary>
    /// Calculates the next billing date based on the start date, billing interval, and interval count.
    /// The start date is set to the current UTC date and time.
    /// </summary>
    /// <param name="interval">Billing interval (e.g., Month, Year, Week, Day).</param>
    /// <param name="intervalCount">Number of intervals between billings.</param>
    /// <returns>The DateTime representing the next billing date.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the billing interval is not recognized.</exception>
    public static DateTime GetNextBillingDate(BillingInterval interval, int intervalCount)
    {
        var startDate = DateTime.UtcNow;
        return interval switch
        {
            BillingInterval.Month => startDate.AddMonths(intervalCount),
            BillingInterval.Year => startDate.AddYears(intervalCount),
            BillingInterval.Week => startDate.AddDays(7 * intervalCount),
            BillingInterval.Day => startDate.AddDays(intervalCount),
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
        };
    }
}
