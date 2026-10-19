using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

public class AgentSessionMappingTests
{
    /// <summary>
    /// The quota sum filters on the audit start time now that the duplicate started_at is gone, so
    /// the covering index has to move with it.
    /// </summary>
    [Fact]
    public void QuotaIndex_CoversCostOnCreatedAt()
    {
        using var db = new TenantDbContext();
        var table = db.GetService<IDesignTimeModel>().Model.GetRelationalModel().FindTable("agent_sessions", null)!;

        Assert.DoesNotContain(table.Columns, c => c.Name is "started_at" or "summary");
        var index = Assert.Single(table.Indexes, i => i.Columns.Single().Name == "created_at");
        Assert.Equal(["EstimatedCostUsd"], index.MappedIndexes.Single().GetIncludeProperties());
    }
}
