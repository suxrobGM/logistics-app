using Logistics.Application.Modules.Integrations.AICopilot.Queries;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class GetAICopilotConversationByIdHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly GetAICopilotConversationByIdHandler sut;

    public GetAICopilotConversationByIdHandlerTests()
    {
        sut = new GetAICopilotConversationByIdHandler(ctx.Queries, ctx.CurrentUser);
        ctx.DecisionRepo.Query().Returns(new List<AgentDecision>().BuildMock());
        ctx.MessageRepo.Query().Returns(new List<AgentMessage>().BuildMock());
    }

    [Fact]
    public async Task Handle_ConversationNotFound_Fails()
    {
        var result = await sut.Handle(new GetAICopilotConversationByIdQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    /// <summary>Another user's conversation must never be readable, even with the right id.</summary>
    [Fact]
    public async Task Handle_OtherUsersConversation_Fails()
    {
        var conversation = ctx.SetConversation(createdById: Guid.NewGuid(), kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(new GetAICopilotConversationByIdQuery { Id = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_DispatchKindConversation_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);

        var result = await sut.Handle(new GetAICopilotConversationByIdQuery { Id = conversation.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    /// <summary>Null DisplayText marks a tool-result row - provider replay data the UI never renders.</summary>
    [Fact]
    public async Task Handle_Success_FiltersOutMessagesWithoutDisplayText()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);
        var textMessage = new AgentMessage { ConversationId = conversation.Id, Sequence = 1, DisplayText = "hello" };
        var toolResultMessage = new AgentMessage { ConversationId = conversation.Id, Sequence = 2, DisplayText = null };
        ctx.MessageRepo.Query().Returns(new List<AgentMessage> { textMessage, toolResultMessage }.BuildMock());

        var result = await sut.Handle(new GetAICopilotConversationByIdQuery { Id = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var message = Assert.Single(result.Value!.Messages!);
        Assert.Equal(textMessage.Id, message.Id);
    }

    [Fact]
    public async Task Handle_Success_IncludesDecisionsForTheConversation()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);
        var session = new AgentSession { ConversationId = conversation.Id, Type = AgentSessionType.Copilot };
        var decision = new AgentDecision { SessionId = session.Id, Session = session, ToolName = "send_invoice" };
        ctx.DecisionRepo.Query().Returns(new List<AgentDecision> { decision }.BuildMock());

        var result = await sut.Handle(new GetAICopilotConversationByIdQuery { Id = conversation.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returnedDecision = Assert.Single(result.Value!.Decisions!);
        Assert.Equal(decision.Id, returnedDecision.Id);
        Assert.Null(result.Value.Sessions);
    }
}
