using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AiDispatch.Commands;
using Logistics.Application.Modules.Integrations.AiDispatch.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AiDispatch;

public class AiDispatchPolicyHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository<AiDispatchPolicy, Guid> policyRepo =
        Substitute.For<ITenantRepository<AiDispatchPolicy, Guid>>();
    private readonly ITenantRepository<Employee, Guid> employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();

    public AiDispatchPolicyHandlerTests()
    {
        tenantUow.Repository<AiDispatchPolicy>().Returns(policyRepo);
        tenantUow.Repository<Employee>().Returns(employeeRepo);
        SetPolicy(null);
    }

    #region Get

    /// <summary>
    /// A tenant that has never run the agent has no row. Returning a blank enabled policy keeps the
    /// null branch out of the controller, the generated client and the page.
    /// </summary>
    [Fact]
    public async Task Get_NoRow_ReturnsBlankEnabledPolicy()
    {
        var sut = new GetAiDispatchPolicyHandler(tenantUow);

        var result = await sut.Handle(new GetAiDispatchPolicyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsEnabled);
        Assert.Null(result.Value.GeneratedContent);
        Assert.Null(result.Value.ManualContent);
    }

    [Fact]
    public async Task Get_ExistingRow_ReturnsBothSections()
    {
        var policy = new AiDispatchPolicy();
        policy.ApplyLearnedPolicy("- Learned rule", 20, DateTime.UtcNow, "deepseek-v4-flash", 0.001m);
        policy.EditManual("- My rule", isEnabled: true, Guid.NewGuid());
        SetPolicy(policy);

        var sut = new GetAiDispatchPolicyHandler(tenantUow);

        var result = await sut.Handle(new GetAiDispatchPolicyQuery(), CancellationToken.None);

        Assert.Equal("- Learned rule", result.Value!.GeneratedContent);
        Assert.Equal("- My rule", result.Value.ManualContent);
        Assert.Equal(20, result.Value.DecisionsAnalyzed);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_NoRow_CreatesOneAndStampsEditor()
    {
        var userId = Guid.NewGuid();
        currentUser.GetUserId().Returns(userId);

        AiDispatchPolicy? added = null;
        await policyRepo.AddAsync(Arg.Do<AiDispatchPolicy>(p => added = p), Arg.Any<CancellationToken>());

        var sut = new UpdateAiDispatchPolicyHandler(tenantUow, currentUser);

        var result = await sut.Handle(
            new UpdateAiDispatchPolicyCommand { ManualContent = "- Prefer flatbeds", IsEnabled = true },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal("- Prefer flatbeds", added!.ManualContent);
        Assert.Equal(userId, added.LastEditedByUserId);
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Editing directives must not disturb what the job learned.</summary>
    [Fact]
    public async Task Update_ExistingRow_LeavesGeneratedContentAlone()
    {
        var policy = new AiDispatchPolicy();
        policy.ApplyLearnedPolicy("- Learned rule", 20, DateTime.UtcNow, "deepseek-v4-flash", 0.001m);
        SetPolicy(policy);

        var sut = new UpdateAiDispatchPolicyHandler(tenantUow, currentUser);

        await sut.Handle(
            new UpdateAiDispatchPolicyCommand { ManualContent = "- My rule", IsEnabled = false },
            CancellationToken.None);

        Assert.Equal("- Learned rule", policy.GeneratedContent);
        Assert.Equal("- My rule", policy.ManualContent);
        Assert.False(policy.IsEnabled);
        await policyRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Update_BlankContent_ClearsDirectives()
    {
        var policy = new AiDispatchPolicy();
        policy.EditManual("- Old rule", isEnabled: true, Guid.NewGuid());
        SetPolicy(policy);

        var sut = new UpdateAiDispatchPolicyHandler(tenantUow, currentUser);

        await sut.Handle(
            new UpdateAiDispatchPolicyCommand { ManualContent = "   ", IsEnabled = true },
            CancellationToken.None);

        Assert.Null(policy.ManualContent);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_ExistingRow_RemovesIt()
    {
        var policy = new AiDispatchPolicy();
        SetPolicy(policy);

        var sut = new DeleteAiDispatchPolicyHandler(tenantUow);

        var result = await sut.Handle(new DeleteAiDispatchPolicyCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        policyRepo.Received(1).Delete(policy);
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_NoRow_SucceedsWithoutSaving()
    {
        var sut = new DeleteAiDispatchPolicyHandler(tenantUow);

        var result = await sut.Handle(new DeleteAiDispatchPolicyCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await tenantUow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    #endregion

    #region Mapper

    /// <summary>
    /// Plans differ by quota, not model tier, and tenants never see model names - so the DTO must not
    /// carry the model or the generation cost.
    /// </summary>
    [Fact]
    public void ToDto_OmitsModelAndCost()
    {
        var policy = new AiDispatchPolicy();
        policy.ApplyLearnedPolicy("- Learned rule", 20, DateTime.UtcNow, "claude-opus-4-8", 12.34m);

        var dto = policy.ToDto();
        var propertyNames = dto.GetType().GetProperties().Select(p => p.Name).ToList();

        Assert.DoesNotContain("ModelUsed", propertyNames);
        Assert.DoesNotContain("GenerationCostUsd", propertyNames);
    }

    #endregion

    private void SetPolicy(AiDispatchPolicy? policy)
    {
        var list = policy is null ? new List<AiDispatchPolicy>() : [policy];
        policyRepo.Query().Returns(list.BuildMock());
    }
}
