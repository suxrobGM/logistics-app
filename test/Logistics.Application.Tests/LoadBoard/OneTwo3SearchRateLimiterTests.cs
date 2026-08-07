using Logistics.Infrastructure.Integrations.LoadBoard;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.OneTwo3;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

public class OneTwo3SearchRateLimiterTests
{
    private readonly TestTimeProvider time = new(new DateTime(2026, 8, 6, 10, 0, 0, DateTimeKind.Utc));
    private readonly Guid configId = Guid.NewGuid();
    private readonly InMemoryOneTwo3SearchRateLimiter sut;

    public OneTwo3SearchRateLimiterTests()
    {
        var options = Options.Create(new LoadBoardOptions
        {
            OneTwo3Loadboard = new OneTwo3LoadboardOptions
            {
                MaxSearchesPerHour = 3,
                MaxSearchesPerDay = 5,
                MaxSearchesPerMonth = 8
            }
        });
        sut = new InMemoryOneTwo3SearchRateLimiter(
            options, time, NullLogger<InMemoryOneTwo3SearchRateLimiter>.Instance);
    }

    private int Acquire(int attempts, Guid? id = null)
    {
        var granted = 0;
        for (var i = 0; i < attempts; i++)
        {
            if (sut.TryAcquireSearch(id ?? configId))
            {
                granted++;
            }
        }

        return granted;
    }

    [Fact]
    public void TryAcquireSearch_HourlyCeiling_BlocksFourthSearch()
    {
        Assert.Equal(3, Acquire(4));
    }

    [Fact]
    public void TryAcquireSearch_HourRolls_ResetsHourlyOnly()
    {
        Acquire(3);
        time.Advance(TimeSpan.FromHours(1));

        // Daily ceiling (5) still applies: only 2 remain despite the fresh hour window
        Assert.Equal(2, Acquire(3));
    }

    [Fact]
    public void TryAcquireSearch_WithinSameHour_DoesNotReset()
    {
        Acquire(3);
        time.Advance(TimeSpan.FromMinutes(30));

        Assert.Equal(0, Acquire(1));
    }

    [Fact]
    public void TryAcquireSearch_MonthlyCeiling_Blocks()
    {
        Assert.Equal(3, Acquire(3));                  // month: 3
        time.Advance(TimeSpan.FromHours(1));
        Assert.Equal(2, Acquire(3));                  // day cap 5 → month: 5
        time.Advance(TimeSpan.FromDays(1));
        Assert.Equal(3, Acquire(3));                  // month cap 8 reached
        time.Advance(TimeSpan.FromDays(1));

        Assert.Equal(0, Acquire(5));

        // A new month resets the ceiling
        time.Set(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal(3, Acquire(3));
    }

    [Fact]
    public void TryAcquireSearch_DistinctConfigurations_MeteredIndependently()
    {
        Acquire(3);

        Assert.Equal(3, Acquire(3, Guid.NewGuid()));
    }

    private sealed class TestTimeProvider(DateTime start) : TimeProvider
    {
        private DateTimeOffset now = start;

        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan by) => now = now.Add(by);

        public void Set(DateTime to) => now = to;
    }
}
