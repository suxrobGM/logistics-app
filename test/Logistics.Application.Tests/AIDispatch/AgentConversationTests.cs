using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class AgentConversationTests
{
    /// <summary>
    /// Touching the navigation lazy-loads the whole transcript, on every send and every turn row.
    /// </summary>
    [Fact]
    public void AddMethods_NeverTouchTheMessagesNavigation()
    {
        var conversation = new AgentConversation();

        conversation.AddTextMessage(1, AgentMessageRole.User, "hi");
        conversation.AddTurnMessage(2, Guid.NewGuid(), AgentMessageRole.Assistant, "[]", "hello");

        Assert.Empty(conversation.Messages);
    }

    /// <summary>
    /// A turn used to allocate from the counter it loaded at turn start, so a decision note or broker
    /// reply saved mid-turn took the same sequence and the turn's save hit the unique index.
    /// </summary>
    [Fact]
    public async Task ReserveSequencesAsync_Block_AdvancesTheCounterInTheDatabaseAndReturnsTheFirst()
    {
        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        var conversation = new AgentConversation();
        FormattableString? sql = null;
        tenantUow.SqlQueryAsync<int>(Arg.Do<FormattableString>(s => sql = s), Arg.Any<CancellationToken>())
            .Returns([7]);

        var first = await tenantUow.ReserveSequencesAsync(conversation, 3, CancellationToken.None);

        Assert.Equal(5, first);
        Assert.NotNull(sql);
        Assert.Contains("SET last_sequence = last_sequence + {0}", sql.Format);
        Assert.Contains("RETURNING last_sequence", sql.Format);
        Assert.Equal([3, conversation.Id], sql.GetArguments());
    }
}
