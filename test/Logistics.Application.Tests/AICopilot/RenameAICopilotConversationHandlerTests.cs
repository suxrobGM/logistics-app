using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class RenameAICopilotConversationHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly RenameAICopilotConversationHandler sut;

    public RenameAICopilotConversationHandlerTests()
    {
        sut = new RenameAICopilotConversationHandler(ctx.TenantUow, ctx.CurrentUser);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_Fails()
    {
        var result = await sut.Handle(
            new RenameAICopilotConversationCommand { ConversationId = Guid.NewGuid(), Title = "New title" },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_NotOwner_Fails()
    {
        var conversation = ctx.SetConversation(createdById: Guid.NewGuid(), kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(
            new RenameAICopilotConversationCommand { ConversationId = conversation.Id, Title = "New title" },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_DispatchKindConversation_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);

        var result = await sut.Handle(
            new RenameAICopilotConversationCommand { ConversationId = conversation.Id, Title = "New title" },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_Success_TrimsTitleAndSaves()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(
            new RenameAICopilotConversationCommand { ConversationId = conversation.Id, Title = "  Invoice batch  " },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Invoice batch", conversation.Title);
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
