using Logistics.Domain.Entities;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Tests.Persistence.Services;

/// <summary>
/// Forces translation of the grouped queries in <c>ProfitabilityService</c>, whose own tests run on
/// MockQueryable and evaluate them in memory.
/// </summary>
public class ProfitabilityQueryTranslationTests
{
    private static readonly DateTime From = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = From.AddMonths(1);

    // Copies of the service's queries. This project references Persistence only, so it cannot call the service.
    private static IQueryable<object> TripDrivers(TenantDbContext db) =>
        db.Set<TripStop>()
            .Where(s => s.Load.DeliveredAt >= From && s.Load.DeliveredAt < To)
            .GroupBy(s => new { s.LoadId, s.TripId, s.Trip.DriverId, s.Trip.SecondaryDriverId })
            .Select(g => new
            {
                g.Key.LoadId,
                g.Key.DriverId,
                g.Key.SecondaryDriverId,
                DepartedStops = g.Count(s => s.DepartedAt != null)
            });

    private static IQueryable<object> FundedFactoringFees(TenantDbContext db) =>
        db.Set<FactoringBatchItem>()
            .Where(f => f.FeeAmount != null &&
                        f.Invoice.Load.DeliveredAt >= From && f.Invoice.Load.DeliveredAt < To)
            .GroupBy(f => f.Invoice.LoadId)
            .Select(g => new { LoadId = g.Key, Fee = g.Sum(f => f.FeeAmount!.Value) });

    [Fact]
    public void TripDrivers_GroupInSql()
    {
        using var db = new TenantDbContext();

        var sql = TripDrivers(db).ToQueryString();

        Assert.Contains("GROUP BY", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FundedFactoringFees_GroupInSql()
    {
        using var db = new TenantDbContext();

        var sql = FundedFactoringFees(db).ToQueryString();

        Assert.Contains("GROUP BY", sql, StringComparison.OrdinalIgnoreCase);
    }
}
