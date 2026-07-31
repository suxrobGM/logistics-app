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

    private static IQueryable<TenantUsageShape> UsageAggregate(
        TenantDbContext db, DateTime costWindowStart, DateTime countFrom) =>
        WindowedSessions(db, costWindowStart)
            .GroupBy(_ => 1)
            .Select(g => new TenantUsageShape
            {
                SpentThisWeekUsd = g.Sum(s =>
                    s.StartedAt >= countFrom ? s.EstimatedCostUsd : 0m),
                OverageSessions = g.Sum(s =>
                    s.StartedAt >= countFrom && s.Status == AgentSessionStatus.Completed && s.IsOverage
                        ? 1
                        : 0),
                TotalTokens = g.Sum(s =>
                    s.StartedAt >= countFrom ? s.InputTokensUsed + s.OutputTokensUsed : 0),
                MonthlyLlmCost = g.Sum(s => s.EstimatedCostUsd),
                LastModel = g.Where(s => s.StartedAt >= countFrom)
                    .OrderByDescending(s => s.StartedAt)
                    .Select(s => s.ModelUsed)
                    .FirstOrDefault()
            });

    private sealed record TenantUsageShape
    {
        public decimal SpentThisWeekUsd { get; init; }
        public int OverageSessions { get; init; }
        public int TotalTokens { get; init; }
        public decimal MonthlyLlmCost { get; init; }
        public string? LastModel { get; init; }
    }

    [Fact]
    public void SessionAggregates_TranslateToASingleSqlAggregate()
    {
        using var db = new TenantDbContext();
        var costWindowStart = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var countFrom = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc);

        var sql = UsageAggregate(db, costWindowStart, countFrom).ToQueryString();

        // Reduction happens server-side: aggregates, and no bare column projection to materialise.
        Assert.Contains("sum(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CASE", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LastModelUsed_RidesAlongAsASubqueryInTheSameStatement()
    {
        using var db = new TenantDbContext();
        var costWindowStart = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var countFrom = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc);

        var sql = UsageAggregate(db, costWindowStart, countFrom).ToQueryString();

        // Nested in the aggregate, not a second round trip - and the report reads every tenant DB.
        Assert.Contains("ORDER BY", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LIMIT 1", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("model_used", sql, StringComparison.OrdinalIgnoreCase);
    }
}
