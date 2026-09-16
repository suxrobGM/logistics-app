using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Agents;

/// <summary>
/// These handlers only delegate, so the one thing left to get wrong per surface is the scope they
/// pass. The behaviour behind it lives in <see cref="AgentConversationCommandsTests"/>.
/// </summary>
public class AgentSurfaceScopeTests
{
    private readonly AgentTestContext _ctx = new();

    [Fact]
    public async Task CreateDispatchConversation_UsesDispatchKind()
    {
        AgentConversation? added = null;
        await _ctx.ConversationRepo.AddAsync(
            Arg.Do<AgentConversation>(c => added = c), Arg.Any<CancellationToken>());

        await new CreateAIDispatchConversationHandler(_ctx.Commands, _ctx.CurrentUser)
            .Handle(new CreateAIDispatchConversationCommand(), CancellationToken.None);

        Assert.Equal(AgentConversationKind.Dispatch, added!.Kind);
    }

    [Fact]
    public async Task CreateCopilotConversation_UsesCopilotKind()
    {
        AgentConversation? added = null;
        await _ctx.ConversationRepo.AddAsync(
            Arg.Do<AgentConversation>(c => added = c), Arg.Any<CancellationToken>());

        await new CreateAICopilotConversationHandler(_ctx.Commands, _ctx.CurrentUser)
            .Handle(new CreateAICopilotConversationCommand(), CancellationToken.None);

        Assert.Equal(AgentConversationKind.Copilot, added!.Kind);
    }

    [Fact]
    public async Task RenameDispatchConversation_RejectsACopilotConversation()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Copilot);

        var result = await new RenameAIDispatchConversationHandler(_ctx.Commands).Handle(
            new RenameAIDispatchConversationCommand { ConversationId = conversation.Id, Title = "x" },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RenameCopilotConversation_RejectsAnotherUsersConversation()
    {
        var conversation = _ctx.SetConversation(
            createdById: Guid.NewGuid(), kind: AgentConversationKind.Copilot);

        var result = await new RenameAICopilotConversationHandler(_ctx.Commands, _ctx.CurrentUser).Handle(
            new RenameAICopilotConversationCommand { ConversationId = conversation.Id, Title = "x" },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    /// <summary>Tenant-shared: a dispatch conversation someone else opened is still deletable.</summary>
    [Fact]
    public async Task DeleteDispatchConversation_AllowsAnotherUsersConversation()
    {
        var conversation = _ctx.SetConversation(
            createdById: Guid.NewGuid(), kind: AgentConversationKind.Dispatch);

        var result = await new DeleteAIDispatchConversationHandler(_ctx.Commands).Handle(
            new DeleteAIDispatchConversationCommand { ConversationId = conversation.Id },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteCopilotConversation_RejectsAnotherUsersConversation()
    {
        var conversation = _ctx.SetConversation(
            createdById: Guid.NewGuid(), kind: AgentConversationKind.Copilot);

        var result = await new DeleteAICopilotConversationHandler(_ctx.Commands, _ctx.CurrentUser).Handle(
            new DeleteAICopilotConversationCommand { ConversationId = conversation.Id },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        _ctx.ConversationRepo.DidNotReceiveWithAnyArgs().Delete(default);
    }
}
