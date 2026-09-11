using Logistics.Domain.Entities;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

/// <summary>
/// Forces translation of the three list-search predicates via <c>ToQueryString</c>, the same way
/// <see cref="TenantQuotaUsageQueryTranslationTests"/> does. The handler tests build their data
/// with MockQueryable, which evaluates predicates in memory, so they cannot catch a predicate that
/// only PostgreSQL rejects.
/// </summary>
public class SearchPredicateQueryTranslationTests
{
    // Copies of the predicates in GetExpensesHandler, GetPortalLoadsHandler and
    // GetPortalInvoicesHandler. This project references Persistence only, so it cannot call them.
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
    /// Control. Without it the three tests above would still pass if <c>ToQueryString</c> stopped
    /// forcing translation. This is the shape the handlers used before, and it must stay rejected.
    /// </summary>
    [Fact]
    public void TheStringComparisonOverload_IsStillUntranslatable()
    {
        using var db = new TenantDbContext();

        var ex = Assert.ThrowsAny<InvalidOperationException>(() =>
            db.Set<Expense>()
                .Where(e => (e.VendorName ?? "").Contains("acme", StringComparison.CurrentCultureIgnoreCase))
                .ToQueryString());

        Assert.Contains("could not be translated", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
