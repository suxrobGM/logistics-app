using Logistics.Domain.Entities;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

/// <summary>
/// The expense list and both customer-portal lists filter on a free-text search term in the
/// database. EF Core only rejects an untranslatable predicate at execution time, so these tests
/// force translation via <c>ToQueryString</c> - the same technique
/// <see cref="TenantQuotaUsageQueryTranslationTests"/> uses, and for the same reason: a predicate
/// that stops translating surfaces as a production exception on a shipped page, never as a red test.
///
/// This matters more than usual for search predicates, because the handler tests for these queries
/// build their data with MockQueryable - LINQ to Objects - which evaluates every predicate in
/// memory. <c>string.Contains(value, StringComparison)</c> works perfectly there and throws against
/// PostgreSQL, so in-memory tests cannot distinguish a working search from a broken one by
/// construction. That is exactly how the shape these tests now pin got into three handlers at once.
/// </summary>
public class SearchPredicateQueryTranslationTests
{
    // These mirror the predicates in GetExpensesHandler, GetPortalLoadsHandler and
    // GetPortalInvoicesHandler. They are copies, which is the known cost of testing a query shape
    // without booting the Application layer (this project references Persistence only) - the
    // control test at the bottom is what keeps the copies honest by proving the assertion can fail.
    private static IQueryable<Expense> ExpenseSearch(TenantDbContext db, string search) =>
        db.Set<Expense>().Where(e =>
            (e.VendorName ?? "").ToLower().Contains(search) ||
            (e.Notes ?? "").ToLower().Contains(search));

    private static IQueryable<Load> PortalLoadSearch(TenantDbContext db, string search) =>
        db.Set<Load>().Where(l =>
            l.Name.ToLower().Contains(search) ||
            l.Number.ToString().Contains(search));

    private static IQueryable<LoadInvoice> PortalInvoiceSearch(TenantDbContext db, string search) =>
        db.Set<LoadInvoice>().Where(i =>
            i.Number.ToString().Contains(search) ||
            (i.Load != null && i.Load.Name.ToLower().Contains(search)));

    [Fact]
    public void ExpenseSearch_TranslatesToACaseInsensitiveSqlFilter()
    {
        using var db = new TenantDbContext();

        var sql = ExpenseSearch(db, "acme").ToQueryString();

        // Folded in SQL, not in memory - the whole point is that the database does the matching.
        Assert.Contains("lower(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LIKE", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PortalLoadSearch_TranslatesToACaseInsensitiveSqlFilter()
    {
        using var db = new TenantDbContext();

        var sql = PortalLoadSearch(db, "acme").ToQueryString();

        Assert.Contains("lower(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LIKE", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PortalInvoiceSearch_TranslatesToACaseInsensitiveSqlFilter()
    {
        using var db = new TenantDbContext();

        var sql = PortalInvoiceSearch(db, "acme").ToQueryString();

        Assert.Contains("lower(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LIKE", sql, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The control, and the reason the three tests above are worth anything: without it they would
    /// pass just as happily if <c>ToQueryString</c> had quietly stopped forcing translation. This
    /// is the exact shape the three handlers used before - <c>Contains(value, StringComparison)</c>
    /// inside a <c>Where</c> - and it must still be rejected.
    /// </summary>
    [Fact]
    public void TheStringComparisonOverload_IsStillUntranslatable_SoTheTestsAboveAreNotVacuous()
    {
        using var db = new TenantDbContext();

        var ex = Assert.ThrowsAny<InvalidOperationException>(() =>
            db.Set<Expense>()
                .Where(e => (e.VendorName ?? "").Contains("acme", StringComparison.CurrentCultureIgnoreCase))
                .ToQueryString());

        Assert.Contains("could not be translated", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
