using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class GetLoadsSummaryHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly GetLoadsSummaryHandler _sut;

    public GetLoadsSummaryHandlerTests()
    {
        _sut = new GetLoadsSummaryHandler(_tenantUow, Substitute.For<ICurrentUserService>());
    }

    [Fact]
    public async Task Handle_FilteredLoads_CountsEveryMatchNotOnePage()
    {
        var draft1 = TestData.Load(deliveryCost: 100m);
        var draft2 = TestData.Load(deliveryCost: 200m);
        var dispatched = TestData.Load(deliveryCost: 400m);
        dispatched.AssignedTruckId = Guid.NewGuid();
        dispatched.UpdateStatus(LoadStatus.Dispatched);
        _tenantUow.Stub<Load>(draft1, draft2, dispatched);

        var result = await _sut.Handle(new GetLoadsSummaryQuery(), default);

        Assert.Equal(3, result.Value!.TotalCount);
        Assert.Equal(700m, result.Value.TotalRevenue);
        Assert.Equal(2, result.Value.StatusCounts.Single(c => c.Status == LoadStatus.Draft).Count);
        Assert.Equal(1, result.Value.StatusCounts.Single(c => c.Status == LoadStatus.Dispatched).Count);
    }

    [Fact]
    public async Task Handle_UnassignedFilter_AppliesTheListFilters()
    {
        var unassigned = TestData.Load(deliveryCost: 100m);
        var assigned = TestData.Load(deliveryCost: 200m);
        assigned.AssignedTruckId = Guid.NewGuid();
        _tenantUow.Stub<Load>(unassigned, assigned);

        var result = await _sut.Handle(new GetLoadsSummaryQuery { Unassigned = true }, default);

        Assert.Equal(1, result.Value!.TotalCount);
        Assert.Equal(100m, result.Value.TotalRevenue);
    }
}
