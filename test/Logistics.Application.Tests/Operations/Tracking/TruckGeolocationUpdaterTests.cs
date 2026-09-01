using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Tracking;

public class TruckGeolocationUpdaterTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Truck, Guid> truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly TruckGeolocationUpdater sut;

    private readonly Guid tenantId = Guid.NewGuid();
    private readonly Guid driverId = Guid.NewGuid();
    private readonly Guid truckId = Guid.NewGuid();

    public TruckGeolocationUpdaterTests()
    {
        tenantUow.Repository<Truck>().Returns(truckRepo);
        sut = new TruckGeolocationUpdater(Substitute.For<IMediator>(), tenantUow);
    }

    private void TruckIs(Truck? truck) =>
        truckRepo.GetByIdAsync(truckId, Arg.Any<CancellationToken>()).Returns(truck);

    private Truck TruckDrivenBy(Guid? mainDriverId, Guid? secondaryDriverId = null) => new()
    {
        Id = truckId,
        Number = "101",
        Type = TruckType.FreightTruck,
        MainDriverId = mainDriverId,
        SecondaryDriverId = secondaryDriverId
    };

    [Fact]
    public async Task CanDriverReportForTruck_MainDriver_IsAllowed()
    {
        TruckIs(TruckDrivenBy(driverId));

        Assert.True(await sut.CanDriverReportForTruckAsync(tenantId, truckId, driverId));
        await tenantUow.Received(1).SetCurrentTenantByIdAsync(tenantId);
    }

    [Fact]
    public async Task CanDriverReportForTruck_SecondaryDriver_IsAllowed()
    {
        TruckIs(TruckDrivenBy(Guid.NewGuid(), driverId));

        Assert.True(await sut.CanDriverReportForTruckAsync(tenantId, truckId, driverId));
    }

    [Fact]
    public async Task CanDriverReportForTruck_SomeoneElsesTruck_IsRejected()
    {
        TruckIs(TruckDrivenBy(Guid.NewGuid()));

        Assert.False(await sut.CanDriverReportForTruckAsync(tenantId, truckId, driverId));
    }

    [Fact]
    public async Task CanDriverReportForTruck_UnknownTruck_IsRejected()
    {
        TruckIs(null);

        Assert.False(await sut.CanDriverReportForTruckAsync(tenantId, truckId, driverId));
    }
}
