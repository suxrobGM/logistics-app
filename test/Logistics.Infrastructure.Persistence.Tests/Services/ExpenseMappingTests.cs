using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

public class ExpenseMappingTests
{
    [Fact]
    public void TruckId_MapsToOneColumnWithOneIndexAndOneForeignKey()
    {
        using var db = new TenantDbContext();
        var table = db.GetService<IDesignTimeModel>().Model.GetRelationalModel().FindTable("expenses", null)!;

        Assert.Empty(table.Columns.Where(c => c.Name.Contains("truck_id") && c.Name != "truck_id"));
        var fk = Assert.Single(table.ForeignKeyConstraints, f => f.PrincipalTable.Name == "trucks");
        Assert.Equal("fk_expenses_trucks_truck_id", fk.Name);
        Assert.Equal("truck_id", Assert.Single(fk.Columns).Name);
        Assert.Equal(ReferentialAction.Restrict, fk.OnDeleteAction);
        var index = Assert.Single(table.Indexes, i => i.Columns.Any(c => c.Name == "truck_id"));
        Assert.Equal("ix_expenses_truck_id", index.Name);
    }
}
