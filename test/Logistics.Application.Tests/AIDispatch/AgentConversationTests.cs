using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class AgentConversationTests
{
    /// <summary>
    /// A turn writing several rows used to advance the counter once, so the next appended note took
    /// a sequence a turn row already held and the save hit the unique index (approve returned 500).
    /// </summary>
    [Fact]
    public void AddTextMessage_AfterMultiRowTurn_TakesUnusedSequence()
    {
        var conversation = new AgentConversation();
        var sessionId = Guid.NewGuid();

        conversation.AddTextMessage(AgentMessageRole.User, "Find a truck for the Phoenix load");
        conversation.AddTurnMessage(sessionId, AgentMessageRole.Assistant, "[]", null);
        conversation.AddTurnMessage(sessionId, AgentMessageRole.User, "[]", null);
        conversation.AddTurnMessage(sessionId, AgentMessageRole.Assistant, "[]", "Summary");
        var note = conversation.AddTextMessage(AgentMessageRole.System, "Approved and executed");

        var sequences = conversation.Messages.Select(m => m.Sequence).ToList();
        Assert.Equal([1, 2, 3, 4, 5], sequences);
        Assert.Equal(5, note.Sequence);
        Assert.Equal(5, conversation.LastSequence);
    }
}
