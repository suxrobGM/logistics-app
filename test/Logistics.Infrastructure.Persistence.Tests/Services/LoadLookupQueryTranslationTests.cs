using Logistics.Domain.Entities;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

/// <summary>
/// Forces translation of the two projections in <c>LoadLookupResolver</c> that read across a
/// navigation. Its handler tests run on MockQueryable, which evaluates them in memory.
/// </summary>
public class LoadLookupQueryTranslationTests
{
    // Copies of the resolver's projections. This project references Persistence only, so it
    // cannot call the resolver.
    private static IQueryable<object> TripRefs(TenantDbContext db, Guid[] loadIds) =>
        db.Set<TripStop>()
            .Where(s => loadIds.Contains(s.LoadId))
            .Select(s => new { s.LoadId, s.Trip.Id, s.Trip.Name, s.Trip.Number });

    private static IQueryable<object> InvoicesWithRows(TenantDbContext db, Guid[] loadIds) =>
        db.Set<LoadInvoice>()
            .Where(i => loadIds.Contains(i.LoadId))
            .Select(i => new { Invoice = i, Payments = i.Payments.ToList(), LineItems = i.LineItems.ToList() });

    [Fact]
    public void TripRefs_TranslateToOneJoinedQuery()
    {
        using var db = new TenantDbContext();

        var sql = TripRefs(db, [Guid.NewGuid()]).ToQueryString();

        Assert.Contains("JOIN", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InvoicesWithPaymentsAndLineItems_TranslateToOneQuery()
    {
        using var db = new TenantDbContext();

        var sql = InvoicesWithRows(db, [Guid.NewGuid()]).ToQueryString();

        Assert.Contains("payments", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("line_items", sql, StringComparison.OrdinalIgnoreCase);
    }
}
