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

        List<AgentMessage> rows =
        [
            conversation.AddTextMessage(AgentMessageRole.User, "Find a truck for the Phoenix load"),
            conversation.AddTurnMessage(sessionId, AgentMessageRole.Assistant, "[]", null),
            conversation.AddTurnMessage(sessionId, AgentMessageRole.User, "[]", null),
            conversation.AddTurnMessage(sessionId, AgentMessageRole.Assistant, "[]", "Summary"),
            conversation.AddTextMessage(AgentMessageRole.System, "Approved and executed")
        ];

        Assert.Equal([1, 2, 3, 4, 5], rows.Select(m => m.Sequence));
        Assert.Equal(5, conversation.LastSequence);
    }

    /// <summary>
    /// Touching the navigation lazy-loads the whole transcript, on every send and every turn row.
    /// </summary>
    [Fact]
    public void AddMethods_NeverTouchTheMessagesNavigation()
    {
        var conversation = new AgentConversation();

        conversation.AddTextMessage(AgentMessageRole.User, "hi");
        conversation.AddTurnMessage(Guid.NewGuid(), AgentMessageRole.Assistant, "[]", "hello");

        Assert.Empty(conversation.Messages);
    }
}
