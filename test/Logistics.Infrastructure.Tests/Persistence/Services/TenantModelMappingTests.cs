using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Persistence.Converters;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Logistics.Infrastructure.Tests.Persistence.Services;

/// <summary>
/// Pins tenant model details a migration diff would not flag as wrong: columns, indexes, foreign
/// keys and converters. Everything is read from the design-time model, so no database is needed.
/// </summary>
public class TenantModelMappingTests
{
    private static ITable Table(string name)
    {
        using var db = new TenantDbContext();
        return db.GetService<IDesignTimeModel>().Model.GetRelationalModel().FindTable(name, null)!;
    }

    #region Agents

    [Theory]
    [InlineData("agent_messages", "session_id", "agent_sessions")]
    [InlineData("negotiation_messages", "agent_decision_id", "agent_decisions")]
    [InlineData("rate_negotiations", "conversation_id", "agent_conversations")]
    public void OptionalAgentLink_IsAForeignKeyThatClearsWhenTheTargetGoes(string table, string column, string principal)
    {
        var fk = Assert.Single(Table(table).ForeignKeyConstraints, f => f.Columns.Single().Name == column);

        Assert.Equal(principal, fk.PrincipalTable.Name);
        Assert.Equal(ReferentialAction.SetNull, fk.OnDeleteAction);
    }

    /// <summary>
    /// The quota sum filters on the audit start time now that the duplicate started_at is gone, so
    /// the covering index has to move with it.
    /// </summary>
    [Fact]
    public void AgentSessions_QuotaIndexCoversCostOnCreatedAt()
    {
        var table = Table("agent_sessions");

        Assert.DoesNotContain(table.Columns, c => c.Name is "started_at" or "summary");
        var index = Assert.Single(table.Indexes, i => i.Columns.Single().Name == "created_at");
        Assert.Equal(["EstimatedCostUsd"], index.MappedIndexes.Single().GetIncludeProperties());
    }

    /// <summary>The counter is a shadow property; the reservation SQL still names the column.</summary>
    [Theory]
    [InlineData("agent_conversations")]
    [InlineData("rate_negotiations")]
    public void LastSequence_StaysARequiredIntegerColumn(string tableName)
    {
        var column = Assert.Single(Table(tableName).Columns, c => c.Name == "last_sequence");

        Assert.Equal("integer", column.StoreType);
        Assert.False(column.IsNullable);
    }

    #endregion

    #region Condition reports

    [Theory]
    [InlineData(nameof(ConditionDefect.PartCategory))]
    [InlineData(nameof(ConditionDefect.Severity))]
    [InlineData(nameof(ConditionDefect.Zone))]
    [InlineData(nameof(ConditionDefect.DamageType))]
    public void ConditionDefectEnumColumns_StoreSnakeCase(string property)
    {
        using var db = new TenantDbContext();

        var converter = db.Model.FindEntityType(typeof(ConditionDefect))!.FindProperty(property)!.GetValueConverter();

        Assert.NotNull(converter);
        Assert.Equal(typeof(SnakeCaseEnumConverter<>), converter.GetType().GetGenericTypeDefinition());
    }

    [Fact]
    public void ConditionDefectPhotoDocumentId_IsAForeignKeyToDocumentsThatNullsOnDelete()
    {
        using var db = new TenantDbContext();

        var fk = Assert.Single(db.Model.FindEntityType(typeof(ConditionDefect))!.GetForeignKeys(),
            f => f.Properties.Single().Name == nameof(ConditionDefect.PhotoDocumentId));

        Assert.Equal(typeof(Document), fk.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);
    }

    #endregion

    #region Expenses

    [Fact]
    public void ExpenseTruckId_MapsToOneColumnWithOneIndexAndOneForeignKey()
    {
        var table = Table("expenses");

        Assert.DoesNotContain(table.Columns, c => c.Name.Contains("truck_id") && c.Name != "truck_id");
        var fk = Assert.Single(table.ForeignKeyConstraints, f => f.PrincipalTable.Name == "trucks");
        Assert.Equal("fk_expenses_trucks_truck_id", fk.Name);
        Assert.Equal("truck_id", Assert.Single(fk.Columns).Name);
        Assert.Equal(ReferentialAction.Restrict, fk.OnDeleteAction);
        var index = Assert.Single(table.Indexes, i => i.Columns.Any(c => c.Name == "truck_id"));
        Assert.Equal("ix_expenses_truck_id", index.Name);
    }

    #endregion

    #region Load vehicles

    /// <summary>
    /// Vehicles are a JSON complex collection on the loads row. Building the query proves EF and
    /// Npgsql accept the mapping, and that reading a load selects the vehicles column with it - no
    /// second query per load.
    /// </summary>
    [Fact]
    public void LoadVehicles_MapAsAComplexCollectionReadWithTheLoad()
    {
        using var db = new TenantDbContext();

        var sql = db.Set<Load>().Where(l => l.Type == LoadType.Vehicle).ToQueryString();

        Assert.NotNull(db.Model.FindEntityType(typeof(Load))!.FindComplexProperty(nameof(Load.Vehicles)));
        Assert.Contains("vehicles", sql, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Notifications

    [Fact]
    public void NotificationReads_UniquePerNotificationAndUser_AndCascadeWithTheNotification()
    {
        var table = Table("notification_reads");

        var unique = Assert.Single(table.Indexes, i => i.IsUnique);
        Assert.Equal(["notification_id", "user_id"], unique.Columns.Select(c => c.Name));
        var fk = Assert.Single(table.ForeignKeyConstraints);
        Assert.Equal("notifications", fk.PrincipalTable.Name);
        Assert.Equal(ReferentialAction.Cascade, fk.OnDeleteAction);
    }

    [Fact]
    public void Notifications_IndexCreatedDate_AndHaveNoSharedReadFlag()
    {
        var table = Table("notifications");

        Assert.Contains(table.Indexes, i => i.Columns.Single().Name == "created_date");
        Assert.DoesNotContain(table.Columns, c => c.Name == "is_read");
    }

    #endregion
}
