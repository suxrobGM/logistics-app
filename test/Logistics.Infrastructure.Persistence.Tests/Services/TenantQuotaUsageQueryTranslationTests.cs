using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

/// <summary>
/// The admin quota report aggregates 30 days of agent sessions in the database rather than in
/// memory. EF Core only fails an untranslatable aggregate at execution time, so these tests force
/// translation via <c>ToQueryString</c> - a shape that silently stops translating would otherwise
/// surface as a production exception on the admin page.
/// </summary>
public class TenantQuotaUsageQueryTranslationTests
{
    private static IQueryable<AgentSession> WindowedSessions(TenantDbContext db, DateTime costWindowStart) =>
        db.Set<AgentSession>()
            .Where(s => s.StartedAt >= costWindowStart);

    [Fact]
    public void SessionAggregates_TranslateToASingleSqlAggregate()
    {
        using var db = new TenantDbContext();
        var costWindowStart = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var countFrom = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc);

        var sql = WindowedSessions(db, costWindowStart)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                SpentThisWeekUsd = g.Sum(s =>
                    s.StartedAt >= countFrom ? s.EstimatedCostUsd : 0m),
                OverageSessions = g.Sum(s =>
                    s.StartedAt >= countFrom && s.Status == AgentSessionStatus.Completed && s.IsOverage
                        ? 1
                        : 0),
                TotalTokens = g.Sum(s =>
                    s.StartedAt >= countFrom ? s.InputTokensUsed + s.OutputTokensUsed : 0),
                MonthlyLlmCost = g.Sum(s => s.EstimatedCostUsd)
            })
            .ToQueryString();

        // Reduction happens server-side: aggregates, and no bare column projection to materialise.
        Assert.Contains("sum(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CASE", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LastModelUsed_TranslatesToAnOrderedSingleRowRead()
    {
        using var db = new TenantDbContext();
        var costWindowStart = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var countFrom = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc);

        // Take(1) stands in for the FirstOrDefaultAsync the handler calls - the limit is applied at
        // execution, so it is not visible in ToQueryString without it.
        var sql = WindowedSessions(db, costWindowStart)
            .Where(s => s.StartedAt >= countFrom)
            .OrderByDescending(s => s.StartedAt)
            .Select(s => s.ModelUsed)
            .Take(1)
            .ToQueryString();

        Assert.Contains("ORDER BY", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LIMIT", sql, StringComparison.OrdinalIgnoreCase);
    }
}
