using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

public class MessageSequenceMappingTests
{
    /// <summary>
    /// The counter left the entities for a shadow property; the reservation SQL still names the column.
    /// </summary>
    [Theory]
    [InlineData("agent_conversations")]
    [InlineData("rate_negotiations")]
    public void LastSequence_StaysARequiredIntegerColumn(string tableName)
    {
        using var db = new TenantDbContext();
        var table = db.GetService<IDesignTimeModel>().Model.GetRelationalModel().FindTable(tableName, null)!;

        var column = Assert.Single(table.Columns, c => c.Name == "last_sequence");
        Assert.Equal("integer", column.StoreType);
        Assert.False(column.IsNullable);
    }
}
