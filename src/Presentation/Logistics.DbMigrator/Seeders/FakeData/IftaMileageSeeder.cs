using System.Security.Cryptography;
using System.Text;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds deterministic per-truck jurisdiction mileage rollups (current + previous quarter)
/// plus a thin sample of GPS breadcrumbs, so the IFTA report and quarter-close job demo with
/// realistic data. US-region corridors only — IFTA is a US/CA program.
/// </summary>
internal class IftaMileageSeeder(ILogger<IftaMileageSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(IftaMileageSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 175;
    public override IReadOnlyList<string> DependsOn => [nameof(TruckSeeder)];

    /// <summary>I-40/I-35 corridor states with a representative city coordinate each.</summary>
    private static readonly (string Region, double Longitude, double Latitude)[] Corridor =
    [
        ("TX", -96.797, 32.7767),
        ("OK", -97.5164, 35.4676),
        ("AR", -92.2896, 34.7465),
        ("TN", -90.049, 35.1495),
        ("MO", -94.5786, 39.0997),
        ("IL", -89.6501, 39.7817)
    ];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<TruckJurisdictionMileage>()
            .CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        // IFTA demo data only makes sense for US-region tenants
        if (context.Region?.Region != Logistics.Domain.Primitives.Enums.Region.US)
        {
            return;
        }

        LogStarting();

        var trucks = await context.TenantUnitOfWork.Repository<Truck>().GetListAsync(ct: cancellationToken);
        if (trucks.Count == 0)
        {
            LogCompleted(0);
            return;
        }

        var rollupRepo = context.TenantUnitOfWork.Repository<TruckJurisdictionMileage>();
        var historyRepo = context.TenantUnitOfWork.Repository<TruckLocationHistory>();

        // Previous quarter start → yesterday covers the prior quarter (for the close job)
        // and the open quarter (for the live report)
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var currentQuarterStart = new DateOnly(today.Year, (today.Month - 1) / 3 * 3 + 1, 1);
        var startDate = currentQuarterStart.AddMonths(-3);

        var count = 0;
        foreach (var truck in trucks)
        {
            for (var date = startDate; date < today; date = date.AddDays(1))
            {
                var seed = Seed($"{truck.Number}|ifta|{date:yyyyMMdd}");

                // Trucks rest ~30% of days
                if (seed % 100 < 30)
                {
                    continue;
                }

                // 2-3 jurisdictions per driving day along the corridor
                var startIndex = (int)(seed % (uint)Corridor.Length);
                var statesToday = 2 + (int)(seed % 2);

                for (var i = 0; i < statesToday; i++)
                {
                    var (region, longitude, latitude) = Corridor[(startIndex + i) % Corridor.Length];
                    var miles = 80 + (int)(Seed($"{truck.Number}|{date:yyyyMMdd}|{region}") % 220);

                    await rollupRepo.AddAsync(new TruckJurisdictionMileage
                    {
                        TruckId = truck.Id,
                        Jurisdiction = new TaxJurisdiction { CountryCode = "US", Region = region },
                        Date = date,
                        Miles = miles
                    }, cancellationToken);

                    // Thin breadcrumb trail (one ping per jurisdiction visit, not full 5-min density)
                    await historyRepo.AddAsync(new TruckLocationHistory
                    {
                        TruckId = truck.Id,
                        Location = new GeoPoint(longitude, latitude),
                        Timestamp = date.ToDateTime(new TimeOnly(6 + i * 4, (int)(seed % 60)), DateTimeKind.Utc)
                    }, cancellationToken);

                    count++;
                }
            }
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private static uint Seed(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return BitConverter.ToUInt32(hash, 0);
    }
}
