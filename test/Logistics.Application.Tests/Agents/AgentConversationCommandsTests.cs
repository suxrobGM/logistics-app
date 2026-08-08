using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Agents;

/// <summary>
/// Create, rename and delete are one implementation both surfaces delegate to, so they are covered
/// once here per scope. Which scope each handler passes is <see cref="AgentSurfaceScopeTests"/>.
/// </summary>
public class AgentConversationCommandsTests
{
    private readonly AgentTestContext ctx = new();

    public static TheoryData<AgentConversationKind> BothSurfaces =>
        [AgentConversationKind.Dispatch, AgentConversationKind.Copilot];

    private AgentConversationScope Scope(AgentConversationKind kind) =>
        kind == AgentConversationKind.Copilot
            ? AgentConversationScope.Copilot(ctx.UserId)
            : AgentConversationScope.Dispatch;

    private static AgentConversationKind Other(AgentConversationKind kind) =>
        kind == AgentConversationKind.Copilot
            ? AgentConversationKind.Dispatch
            : AgentConversationKind.Copilot;

    #region Create

    [Fact]
    public async Task CreateAsync_NotAuthenticated_Fails()
    {
        var result = await ctx.Commands.CreateAsync(
            AgentConversationKind.Copilot, userId: null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await ctx.ConversationRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Theory]
    [MemberData(nameof(BothSurfaces))]
    public async Task CreateAsync_Success_StampsKindAndCreator(AgentConversationKind kind)
    {
        AgentConversation? added = null;
        await ctx.ConversationRepo.AddAsync(
            Arg.Do<AgentConversation>(c => added = c), Arg.Any<CancellationToken>());

        var result = await ctx.Commands.CreateAsync(kind, ctx.UserId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(kind, added!.Kind);
        Assert.Equal(ctx.UserId, added.CreatedById);
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Rename

    [Theory]
    [MemberData(nameof(BothSurfaces))]
    public async Task RenameAsync_ConversationNotFound_Fails(AgentConversationKind kind)
    {
        var result = await ctx.Commands.RenameAsync(
            Scope(kind), Guid.NewGuid(), "New title", CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    /// <summary>A conversation belonging to the other surface must be invisible, not just unwritable.</summary>
    [Theory]
    [MemberData(nameof(BothSurfaces))]
    public async Task RenameAsync_OtherSurfacesConversation_Fails(AgentConversationKind kind)
    {
        var conversation = ctx.SetConversation(kind: Other(kind));

        var result = await ctx.Commands.RenameAsync(
            Scope(kind), conversation.Id, "New title", CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(BothSurfaces))]
    public async Task RenameAsync_Success_TrimsTitleAndSaves(AgentConversationKind kind)
    {
        var conversation = ctx.SetConversation(kind: kind);

        var result = await ctx.Commands.RenameAsync(
            Scope(kind), conversation.Id, "  Plan the week  ", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Plan the week", conversation.Title);
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Delete

    [Theory]
    [MemberData(nameof(BothSurfaces))]
    public async Task DeleteAsync_ConversationNotFound_Fails(AgentConversationKind kind)
    {
        var result = await ctx.Commands.DeleteAsync(
            Scope(kind), Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(BothSurfaces))]
    public async Task DeleteAsync_OtherSurfacesConversation_Fails(AgentConversationKind kind)
    {
        var conversation = ctx.SetConversation(kind: Other(kind));

        var result = await ctx.Commands.DeleteAsync(Scope(kind), conversation.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        ctx.ConversationRepo.DidNotReceiveWithAnyArgs().Delete(default);
    }

    [Theory]
    [MemberData(nameof(BothSurfaces))]
    public async Task DeleteAsync_TurnRunning_Fails(AgentConversationKind kind)
    {
        var conversation = ctx.SetConversation(kind: kind);
        conversation.BeginTurn();

        var result = await ctx.Commands.DeleteAsync(Scope(kind), conversation.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("running", result.Error);
        ctx.ConversationRepo.DidNotReceiveWithAnyArgs().Delete(default);
    }

    [Theory]
    [MemberData(nameof(BothSurfaces))]
    public async Task DeleteAsync_Success_DeletesAndSaves(AgentConversationKind kind)
    {
        var conversation = ctx.SetConversation(kind: kind);

        var result = await ctx.Commands.DeleteAsync(Scope(kind), conversation.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        ctx.ConversationRepo.Received(1).Delete(conversation);
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Ownership

    /// <summary>Copilot conversations are private; dispatch ones are tenant-shared.</summary>
    [Fact]
    public async Task CopilotScope_AnotherUsersConversation_IsNotVisible()
    {
        var conversation = ctx.SetConversation(
            createdById: Guid.NewGuid(), kind: AgentConversationKind.Copilot);

        var renamed = await ctx.Commands.RenameAsync(
            Scope(AgentConversationKind.Copilot), conversation.Id, "Mine now", CancellationToken.None);
        var deleted = await ctx.Commands.DeleteAsync(
            Scope(AgentConversationKind.Copilot), conversation.Id, CancellationToken.None);

        Assert.False(renamed.IsSuccess);
        Assert.False(deleted.IsSuccess);
    }

    [Fact]
    public async Task DispatchScope_AnotherUsersConversation_IsStillWritable()
    {
        var conversation = ctx.SetConversation(
            createdById: Guid.NewGuid(), kind: AgentConversationKind.Dispatch);

        var result = await ctx.Commands.RenameAsync(
            AgentConversationScope.Dispatch, conversation.Id, "Night shift", CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CopilotScope_Unauthenticated_MatchesNoConversation()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);

        var result = await ctx.Commands.RenameAsync(
            AgentConversationScope.Copilot(null), conversation.Id, "New title", CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    #endregion
}
