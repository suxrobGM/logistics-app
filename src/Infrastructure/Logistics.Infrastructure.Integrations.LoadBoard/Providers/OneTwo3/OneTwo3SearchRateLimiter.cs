using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.OneTwo3;

internal interface IOneTwo3SearchRateLimiter
{
    bool TryAcquireSearch(Guid configurationId);
}

internal sealed class InMemoryOneTwo3SearchRateLimiter(
    IOptions<LoadBoardOptions> options,
    TimeProvider timeProvider,
    ILogger<InMemoryOneTwo3SearchRateLimiter> logger) : IOneTwo3SearchRateLimiter
{
    private readonly OneTwo3LoadboardOptions options = options.Value.OneTwo3Loadboard ?? new OneTwo3LoadboardOptions();
    private readonly ConcurrentDictionary<Guid, WindowCounters> counters = new();

    public bool TryAcquireSearch(Guid configurationId)
    {
        var counter = counters.GetOrAdd(configurationId, _ => new WindowCounters());
        var now = timeProvider.GetUtcNow().UtcDateTime;

        lock (counter)
        {
            // Fixed calendar windows (UTC) matching how the vendor resets its quotas
            var hour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
            var month = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            if (counter.HourStart != hour)
            {
                counter.HourStart = hour;
                counter.HourCount = 0;
            }

            if (counter.DayStart != now.Date)
            {
                counter.DayStart = now.Date;
                counter.DayCount = 0;
            }

            if (counter.MonthStart != month)
            {
                counter.MonthStart = month;
                counter.MonthCount = 0;
            }

            if (counter.HourCount >= options.MaxSearchesPerHour ||
                counter.DayCount >= options.MaxSearchesPerDay ||
                counter.MonthCount >= options.MaxSearchesPerMonth)
            {
                logger.LogWarning(
                    "123Loadboard search rate limit reached for configuration {ConfigurationId}: " +
                    "hour {HourCount}/{MaxHour}, day {DayCount}/{MaxDay}, month {MonthCount}/{MaxMonth}",
                    configurationId, counter.HourCount, options.MaxSearchesPerHour,
                    counter.DayCount, options.MaxSearchesPerDay,
                    counter.MonthCount, options.MaxSearchesPerMonth);
                return false;
            }

            counter.HourCount++;
            counter.DayCount++;
            counter.MonthCount++;
            return true;
        }
    }

    private sealed class WindowCounters
    {
        public DateTime HourStart { get; set; }
        public int HourCount { get; set; }
        public DateTime DayStart { get; set; }
        public int DayCount { get; set; }
        public DateTime MonthStart { get; set; }
        public int MonthCount { get; set; }
    }
}
