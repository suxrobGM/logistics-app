using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.Tenancy;
using Logistics.Application.Attributes;
using Logistics.Application.Behaviours;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Behaviours;

public class FeatureCheckBehaviourTests
{
    private readonly IFeatureService _featureService = Substitute.For<IFeatureService>();
    private readonly ICurrentTenantAccessor _tenantAccessor = Substitute.For<ICurrentTenantAccessor>();

    public FeatureCheckBehaviourTests()
    {
        _tenantAccessor.GetCurrentTenant().Returns(TestTenant.Create());
        _featureService.CheckFeatureAsync(Arg.Any<Guid>(), Arg.Any<TenantFeature>())
            .Returns(Result.Fail("Feature not enabled", "FEATURE_DISABLED"));
    }

    [Fact]
    public async Task Handle_FirstFeatureEnabled_RunsHandler()
    {
        Enable(TenantFeature.Safety);

        var result = await Run<SafetyOrVehicleTransportRequest>();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_OnlySecondFeatureEnabled_RunsHandler()
    {
        Enable(TenantFeature.VehicleTransport);

        var result = await Run<SafetyOrVehicleTransportRequest>();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_NoFeatureEnabled_ReturnsFirstFeaturesError()
    {
        _featureService.CheckFeatureAsync(Arg.Any<Guid>(), TenantFeature.Safety)
            .Returns(Result.Fail("Safety is not in your plan", "FEATURE_NOT_IN_PLAN"));

        var result = await Run<SafetyOrVehicleTransportRequest>();

        Assert.False(result.IsSuccess);
        Assert.Equal("Safety is not in your plan", result.Error);
        Assert.Equal("FEATURE_NOT_IN_PLAN", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_SingleFeatureDisabled_SkipsHandler()
    {
        var handlerRan = false;

        var result = await Run<SafetyOnlyRequest>(() => handlerRan = true);

        Assert.False(result.IsSuccess);
        Assert.False(handlerRan);
    }

    private void Enable(TenantFeature feature) =>
        _featureService.CheckFeatureAsync(Arg.Any<Guid>(), feature).Returns(Result.Ok());

    private Task<Result> Run<TRequest>(Action? onHandler = null) where TRequest : notnull, new()
    {
        var sut = new FeatureCheckBehaviour<TRequest, Result>(_featureService, _tenantAccessor);
        return sut.Handle(new TRequest(), _ =>
        {
            onHandler?.Invoke();
            return Task.FromResult(Result.Ok());
        }, CancellationToken.None);
    }

    [RequiresFeature(TenantFeature.Safety, TenantFeature.VehicleTransport)]
    public sealed class SafetyOrVehicleTransportRequest;

    [RequiresFeature(TenantFeature.Safety)]
    public sealed class SafetyOnlyRequest;
}
