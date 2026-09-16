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
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Truck, Guid> _truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly TruckGeolocationUpdater _sut;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _driverId = Guid.NewGuid();
    private readonly Guid _truckId = Guid.NewGuid();

    public TruckGeolocationUpdaterTests()
    {
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _sut = new TruckGeolocationUpdater(Substitute.For<IMediator>(), _tenantUow);
    }

    private void TruckIs(Truck? truck) =>
        _truckRepo.GetByIdAsync(_truckId, Arg.Any<CancellationToken>()).Returns(truck);

    private Truck TruckDrivenBy(Guid? mainDriverId, Guid? secondaryDriverId = null) => new()
    {
        Id = _truckId,
        Number = "101",
        Type = TruckType.FreightTruck,
        MainDriverId = mainDriverId,
        SecondaryDriverId = secondaryDriverId
    };

    [Fact]
    public async Task CanDriverReportForTruck_MainDriver_IsAllowed()
    {
        TruckIs(TruckDrivenBy(_driverId));

        Assert.True(await _sut.CanDriverReportForTruckAsync(_tenantId, _truckId, _driverId));
        await _tenantUow.Received(1).SetCurrentTenantByIdAsync(_tenantId);
    }

    [Fact]
    public async Task CanDriverReportForTruck_SecondaryDriver_IsAllowed()
    {
        TruckIs(TruckDrivenBy(Guid.NewGuid(), _driverId));

        Assert.True(await _sut.CanDriverReportForTruckAsync(_tenantId, _truckId, _driverId));
    }

    [Fact]
    public async Task CanDriverReportForTruck_SomeoneElsesTruck_IsRejected()
    {
        TruckIs(TruckDrivenBy(Guid.NewGuid()));

        Assert.False(await _sut.CanDriverReportForTruckAsync(_tenantId, _truckId, _driverId));
    }

    [Fact]
    public async Task CanDriverReportForTruck_UnknownTruck_IsRejected()
    {
        TruckIs(null);

        Assert.False(await _sut.CanDriverReportForTruckAsync(_tenantId, _truckId, _driverId));
    }
}
