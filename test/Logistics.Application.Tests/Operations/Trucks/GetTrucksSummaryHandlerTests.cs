using Logistics.Application.Modules.Operations.Trucks.Queries;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trucks;

public class GetTrucksSummaryHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly GetTrucksSummaryHandler _sut;

    public GetTrucksSummaryHandlerTests()
    {
        _sut = new GetTrucksSummaryHandler(_tenantUow);
    }

    [Fact]
    public async Task Handle_IdleFilter_LeavesOutTrucksWithALoadOnTheRoad()
    {
        var idle = TestData.Truck(number: "T-1");
        var busy = TestData.Truck(number: "T-2");
        busy.Status = TruckStatus.EnRoute;
        var load = TestData.Load();
        load.UpdateStatus(LoadStatus.Dispatched);
        busy.Loads.Add(load);
        _tenantUow.Stub<Truck>(idle, busy);

        var result = await _sut.Handle(new GetTrucksSummaryQuery { Idle = true }, default);

        var count = Assert.Single(result.Value!.StatusCounts);
        Assert.Equal(TruckStatus.Available, count.Status);
        Assert.Equal(1, result.Value.TotalCount);
    }
}
