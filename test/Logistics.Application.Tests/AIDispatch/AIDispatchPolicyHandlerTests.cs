using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Modules.Integrations.AIDispatch.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class AIDispatchPolicyHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository<AIDispatchPolicy, Guid> _policyRepo =
        Substitute.For<ITenantRepository<AIDispatchPolicy, Guid>>();
    private readonly ITenantRepository<Employee, Guid> _employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();

    public AIDispatchPolicyHandlerTests()
    {
        _tenantUow.Repository<AIDispatchPolicy>().Returns(_policyRepo);
        _tenantUow.Repository<Employee>().Returns(_employeeRepo);
        SetPolicy(null);
    }

    #region Get

    /// <summary>
    /// A tenant that has never run the agent has no row. A blank enabled policy keeps the null branch
    /// out of the controller, the generated client and the page.
    /// </summary>
    [Fact]
    public async Task Get_NoRow_ReturnsBlankEnabledPolicy()
    {
        var sut = new GetAIDispatchPolicyHandler(_tenantUow);

        var result = await sut.Handle(new GetAIDispatchPolicyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsEnabled);
        Assert.Null(result.Value.GeneratedContent);
        Assert.Null(result.Value.ManualContent);
    }

    [Fact]
    public async Task Get_ExistingRow_ReturnsBothSections()
    {
        var policy = new AIDispatchPolicy();
        policy.ApplyLearnedPolicy("- Learned rule", 20, DateTime.UtcNow, "deepseek-v4-flash", 0.001m);
        policy.EditManual("- My rule", isEnabled: true, Guid.NewGuid());
        SetPolicy(policy);

        var sut = new GetAIDispatchPolicyHandler(_tenantUow);

        var result = await sut.Handle(new GetAIDispatchPolicyQuery(), CancellationToken.None);

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
        _currentUser.GetUserId().Returns(userId);

        AIDispatchPolicy? added = null;
        await _policyRepo.AddAsync(Arg.Do<AIDispatchPolicy>(p => added = p), Arg.Any<CancellationToken>());

        var sut = new UpdateAIDispatchPolicyHandler(_tenantUow, _currentUser);

        var result = await sut.Handle(
            new UpdateAIDispatchPolicyCommand { ManualContent = "- Prefer flatbeds", IsEnabled = true },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal("- Prefer flatbeds", added!.ManualContent);
        Assert.Equal(userId, added.LastEditedByUserId);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Editing directives must not disturb what the job learned.</summary>
    [Fact]
    public async Task Update_ExistingRow_LeavesGeneratedContentAlone()
    {
        var policy = new AIDispatchPolicy();
        policy.ApplyLearnedPolicy("- Learned rule", 20, DateTime.UtcNow, "deepseek-v4-flash", 0.001m);
        SetPolicy(policy);

        var sut = new UpdateAIDispatchPolicyHandler(_tenantUow, _currentUser);

        await sut.Handle(
            new UpdateAIDispatchPolicyCommand { ManualContent = "- My rule", IsEnabled = false },
            CancellationToken.None);

        Assert.Equal("- Learned rule", policy.GeneratedContent);
        Assert.Equal("- My rule", policy.ManualContent);
        Assert.False(policy.IsEnabled);
        await _policyRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Update_BlankContent_ClearsDirectives()
    {
        var policy = new AIDispatchPolicy();
        policy.EditManual("- Old rule", isEnabled: true, Guid.NewGuid());
        SetPolicy(policy);

        var sut = new UpdateAIDispatchPolicyHandler(_tenantUow, _currentUser);

        await sut.Handle(
            new UpdateAIDispatchPolicyCommand { ManualContent = "   ", IsEnabled = true },
            CancellationToken.None);

        Assert.Null(policy.ManualContent);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_ExistingRow_RemovesIt()
    {
        var policy = new AIDispatchPolicy();
        SetPolicy(policy);

        var sut = new DeleteAIDispatchPolicyHandler(_tenantUow);

        var result = await sut.Handle(new DeleteAIDispatchPolicyCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _policyRepo.Received(1).Delete(policy);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_NoRow_SucceedsWithoutSaving()
    {
        var sut = new DeleteAIDispatchPolicyHandler(_tenantUow);

        var result = await sut.Handle(new DeleteAIDispatchPolicyCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _tenantUow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    #endregion

    #region Mapper

    /// <summary>
    /// Plans differ by quota, not model tier, so the DTO carries neither model nor generation cost.
    /// </summary>
    [Fact]
    public void ToDto_OmitsModelAndCost()
    {
        var policy = new AIDispatchPolicy();
        policy.ApplyLearnedPolicy("- Learned rule", 20, DateTime.UtcNow, "claude-sonnet-5", 12.34m);

        var dto = policy.ToDto();
        var propertyNames = dto.GetType().GetProperties().Select(p => p.Name).ToList();

        Assert.DoesNotContain("ModelUsed", propertyNames);
        Assert.DoesNotContain("GenerationCostUsd", propertyNames);
    }

    #endregion

    private void SetPolicy(AIDispatchPolicy? policy)
    {
        var list = policy is null ? new List<AIDispatchPolicy>() : [policy];
        _policyRepo.Query().Returns(list.BuildMock());
    }
}
