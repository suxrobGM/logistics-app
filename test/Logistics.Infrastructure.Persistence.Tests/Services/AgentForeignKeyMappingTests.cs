using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

public class AgentForeignKeyMappingTests
{
    [Theory]
    [InlineData("agent_messages", "session_id", "agent_sessions")]
    [InlineData("negotiation_messages", "agent_decision_id", "agent_decisions")]
    [InlineData("rate_negotiations", "conversation_id", "agent_conversations")]
    public void OptionalLink_IsAForeignKeyThatClearsWhenTheTargetGoes(string table, string column, string principal)
    {
        using var db = new TenantDbContext();
        var model = db.GetService<IDesignTimeModel>().Model.GetRelationalModel();

        var fk = Assert.Single(model.FindTable(table, null)!.ForeignKeyConstraints,
            f => f.Columns.Single().Name == column);
        Assert.Equal(principal, fk.PrincipalTable.Name);
        Assert.Equal(ReferentialAction.SetNull, fk.OnDeleteAction);
    }
}
