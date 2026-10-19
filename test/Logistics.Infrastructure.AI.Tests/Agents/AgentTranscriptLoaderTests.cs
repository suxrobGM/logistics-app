using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Agents;
using Logistics.Infrastructure.AI.Llm.Contracts;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Agents;

public class AgentTranscriptLoaderTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<AgentMessage, Guid> _messageRepo =
        Substitute.For<ITenantRepository<AgentMessage, Guid>>();
    private readonly AgentConversation _conversation = new() { Kind = AgentConversationKind.Dispatch };
    private readonly List<AgentMessage> _rows = [];

    public AgentTranscriptLoaderTests()
    {
        _tenantUow.Repository<AgentMessage>().Returns(_messageRepo);
        _messageRepo.Query().Returns(_ => _rows.BuildMock());
    }

    private void UserMessage(string text) => _rows.Add(_conversation.AddTextMessage(_rows.Count + 1, AgentMessageRole.User, text));

    /// <summary>One tool round trip: assistant tool_use, then the user-role tool_result row.</summary>
    private void ToolRound(string result)
    {
        var sessionId = Guid.NewGuid();
        var callId = Guid.NewGuid().ToString();
        _rows.Add(_conversation.AddTurnMessage(_rows.Count + 1, sessionId, AgentMessageRole.Assistant,
            AgentTranscriptCodec.Encode([new LlmToolUseBlock(callId, "get_available_trucks", null)]), null));
        _rows.Add(_conversation.AddTurnMessage(_rows.Count + 1, sessionId, AgentMessageRole.User,
            AgentTranscriptCodec.Encode([new LlmToolResultBlock(callId, result)]), null));
    }

    private Task<List<LlmMessage>> Load() =>
        new AgentTranscriptLoader(_tenantUow).LoadAsync(_conversation, CancellationToken.None);

    #region Window

    [Theory]
    [InlineData(1, 1)]
    [InlineData(40, 1)]
    [InlineData(41, 21)]
    [InlineData(60, 21)] // the start holds still for a whole step...
    [InlineData(61, 41)] // ...then jumps, instead of sliding one row per turn
    public void WindowStart_MovesInWholeSteps(int lastSequence, int expected)
    {
        Assert.Equal(expected, AgentTranscriptLoader.WindowStart(lastSequence));
    }

    [Fact]
    public async Task Load_LongConversation_StartsAtAChatBoundaryAndSaysHistoryWasOmitted()
    {
        // Starting mid-turn would orphan a tool_use/tool_result pair and the provider would reject it.
        for (var turn = 0; turn < 15; turn++)
        {
            UserMessage($"turn {turn}");
            ToolRound("{}");
        }

        var messages = await Load();

        var first = messages[0];
        Assert.Equal(LlmRole.User, first.Role);
        Assert.Equal(AgentTranscriptLoader.OmittedHistoryNote, Assert.IsType<LlmTextBlock>(first.Content[0]).Text);
        Assert.DoesNotContain(first.Content, b => b is LlmToolResultBlock);
        Assert.True(messages.Count < _rows.Count);
    }

    [Fact]
    public async Task Load_ShortConversation_ReplaysEverythingWithoutTheOmittedNote()
    {
        UserMessage("hi");
        ToolRound("{}");

        var messages = await Load();

        Assert.Equal(3, messages.Count);
        Assert.DoesNotContain(messages.SelectMany(m => m.Content),
            b => b is LlmTextBlock { Text: AgentTranscriptLoader.OmittedHistoryNote });
    }

    #endregion

    #region Compaction and stamps

    [Fact]
    public async Task Load_LargeToolResultFromAnEarlierTurn_ReplaysAsAStub()
    {
        UserMessage("plan");
        ToolRound(new string('x', AgentTranscriptLoader.MaxReplayedToolResultChars + 1));

        var result = (await Load()).SelectMany(m => m.Content).OfType<LlmToolResultBlock>().Single();

        Assert.Equal(AgentTranscriptLoader.OmittedToolResult, result.Content);
    }

    [Fact]
    public async Task Load_SmallToolResult_ReplaysAsIs()
    {
        // Errors and a suggestion's status are short and still matter to the next turn.
        UserMessage("plan");
        ToolRound("""{"status":"suggested"}""");

        var result = (await Load()).SelectMany(m => m.Content).OfType<LlmToolResultBlock>().Single();

        Assert.Equal("""{"status":"suggested"}""", result.Content);
    }

    [Fact]
    public async Task Load_ChatMessages_CarryTheirSentStampButToolResultsDoNot()
    {
        UserMessage("plan");
        ToolRound("{}");

        var messages = await Load();

        Assert.Equal(AgentTranscriptLoader.SentStamp(_rows[0].CreatedAt),
            Assert.IsType<LlmTextBlock>(messages[0].Content[0]).Text);
        Assert.IsType<LlmToolResultBlock>(Assert.Single(messages[2].Content));
    }

    #endregion
}
