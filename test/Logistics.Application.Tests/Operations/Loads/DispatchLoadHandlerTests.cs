using Logistics.Application.Abstractions.ApplicationServices;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class DispatchLoadHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IDispatchEligibilityService _eligibility = Substitute.For<IDispatchEligibilityService>();
    private readonly DispatchLoadHandler _sut;

    public DispatchLoadHandlerTests()
    {
        _sut = new DispatchLoadHandler(_tenantUow, _eligibility);
    }

    [Fact]
    public async Task Handle_NoTruckAssigned_FailsWithoutDispatching()
    {
        var load = TestData.Load();
        _tenantUow.Stub<Load>(load);

        var result = await _sut.Handle(new DispatchLoadCommand { Id = load.Id }, default);

        Assert.False(result.IsSuccess);
        Assert.Contains("Assign a truck", result.Error);
        Assert.Equal(LoadStatus.Draft, load.Status);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
