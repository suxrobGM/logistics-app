using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

public class NotificationMappingTests
{
    private static IRelationalModel Model()
    {
        using var db = new TenantDbContext();
        return db.GetService<IDesignTimeModel>().Model.GetRelationalModel();
    }

    [Fact]
    public void NotificationReads_UniquePerNotificationAndUser_AndCascadeWithTheNotification()
    {
        var table = Model().FindTable("notification_reads", null)!;

        var unique = Assert.Single(table.Indexes, i => i.IsUnique);
        Assert.Equal(["notification_id", "user_id"], unique.Columns.Select(c => c.Name));
        var fk = Assert.Single(table.ForeignKeyConstraints);
        Assert.Equal("notifications", fk.PrincipalTable.Name);
        Assert.Equal(ReferentialAction.Cascade, fk.OnDeleteAction);
    }

    [Fact]
    public void Notifications_IndexCreatedDate_AndHaveNoSharedReadFlag()
    {
        var table = Model().FindTable("notifications", null)!;

        Assert.Contains(table.Indexes, i => i.Columns.Single().Name == "created_date");
        Assert.DoesNotContain(table.Columns, c => c.Name == "is_read");
    }
}
