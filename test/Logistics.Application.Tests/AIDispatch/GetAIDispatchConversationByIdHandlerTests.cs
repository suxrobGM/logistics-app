using Logistics.Application.Modules.Integrations.AIDispatch.Queries;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class GetAIDispatchConversationByIdHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly GetAIDispatchConversationByIdHandler sut;

    public GetAIDispatchConversationByIdHandlerTests()
    {
        sut = new GetAIDispatchConversationByIdHandler(ctx.Queries);
        ctx.DecisionRepo.Query().Returns(new List<AgentDecision>().BuildMock());
        ctx.SessionRepo.Query().Returns(new List<AgentSession>().BuildMock());
        ctx.MessageRepo.Query().Returns(new List<AgentMessage>().BuildMock());
    }

    [Fact]
    public async Task Handle_ConversationNotFound_Fails()
    {
        var result = await sut.Handle(new GetAIDispatchConversationByIdQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_CopilotKindConversation_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(new GetAIDispatchConversationByIdQuery { Id = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    /// <summary>Null DisplayText marks a tool-result row - provider replay data the UI never renders.</summary>
    [Fact]
    public async Task Handle_Success_FiltersOutMessagesWithoutDisplayText()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var textMessage = new AgentMessage { ConversationId = conversation.Id, Sequence = 1, DisplayText = "hello" };
        var toolResultMessage = new AgentMessage { ConversationId = conversation.Id, Sequence = 2, DisplayText = null };
        ctx.MessageRepo.Query().Returns(new List<AgentMessage> { textMessage, toolResultMessage }.BuildMock());

        var result = await sut.Handle(new GetAIDispatchConversationByIdQuery { Id = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var message = Assert.Single(result.Value!.Messages!);
        Assert.Equal(textMessage.Id, message.Id);
    }

    [Fact]
    public async Task Handle_Success_NamesTheSenderOfEachMessage()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var otherUserId = Guid.NewGuid();
        ctx.SetEmployees((ctx.UserId, "Sarah", "Thompson"), (otherUserId, "Marcus", "Johnson"));
        ctx.MessageRepo.Query().Returns(new List<AgentMessage>
        {
            new() { ConversationId = conversation.Id, Sequence = 1, DisplayText = "mine", SentByUserId = ctx.UserId },
            new() { ConversationId = conversation.Id, Sequence = 2, DisplayText = "theirs", SentByUserId = otherUserId },
            new() { ConversationId = conversation.Id, Sequence = 3, DisplayText = "the agent" }
        }.BuildMock());

        var result = await sut.Handle(new GetAIDispatchConversationByIdQuery { Id = conversation.Id }, CancellationToken.None);

        var messages = result.Value!.Messages!;
        Assert.Equal("Sarah Thompson", messages[0].SentByName);
        Assert.Equal("Marcus Johnson", messages[1].SentByName);
        Assert.Null(messages[2].SentByName);
        Assert.Null(messages[2].SentByUserId);
    }

    /// <summary>An employee who has left keeps their id on the row, so the UI just shows no name.</summary>
    [Fact]
    public async Task Handle_SenderHasNoEmployeeRow_ReturnsIdWithoutName()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var goneUserId = Guid.NewGuid();
        ctx.MessageRepo.Query().Returns(new List<AgentMessage>
        {
            new() { ConversationId = conversation.Id, Sequence = 1, DisplayText = "hi", SentByUserId = goneUserId }
        }.BuildMock());

        var result = await sut.Handle(new GetAIDispatchConversationByIdQuery { Id = conversation.Id }, CancellationToken.None);

        var message = Assert.Single(result.Value!.Messages!);
        Assert.Equal(goneUserId, message.SentByUserId);
        Assert.Null(message.SentByName);
    }

    [Fact]
    public async Task Handle_Success_IncludesSessionsAndDecisionsForTheConversation()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var session = new AgentSession { ConversationId = conversation.Id, Type = AgentSessionType.Dispatch };
        var decision = new AgentDecision { SessionId = session.Id, Session = session, ToolName = "assign_load_to_truck" };
        var otherSession = new AgentSession { ConversationId = Guid.NewGuid(), Type = AgentSessionType.Dispatch };
        ctx.SessionRepo.Query().Returns(new List<AgentSession> { session, otherSession }.BuildMock());
        ctx.DecisionRepo.Query().Returns(new List<AgentDecision> { decision }.BuildMock());

        var result = await sut.Handle(new GetAIDispatchConversationByIdQuery { Id = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returnedSession = Assert.Single(result.Value!.Sessions!);
        Assert.Equal(session.Id, returnedSession.Id);
        var returnedDecision = Assert.Single(result.Value.Decisions!);
        Assert.Equal(decision.Id, returnedDecision.Id);
    }
}
