using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class AssignLoadToTruckHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Load, Guid> _loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ITenantRepository<Truck, Guid> _truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly AssignLoadToTruckHandler _sut;

    public AssignLoadToTruckHandlerTests()
    {
        _tenantUow.Repository<Load>().Returns(_loadRepo);
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _sut = new AssignLoadToTruckHandler(_tenantUow);
    }

    [Fact]
    public async Task Handle_CompatibleTruck_AssignsAndSaves()
    {
        var (load, truck) = Wire(LoadType.Vehicle, TruckType.CarHauler);

        var result = await _sut.Handle(new AssignLoadToTruckCommand { LoadId = load.Id, TruckId = truck.Id }, default);

        Assert.True(result.IsSuccess);
        Assert.Equal(truck.Id, load.AssignedTruckId);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IncompatibleTruck_FailsWithoutSaving()
    {
        var (load, truck) = Wire(LoadType.IntermodalContainer, TruckType.FreightTruck);

        var result = await _sut.Handle(new AssignLoadToTruckCommand { LoadId = load.Id, TruckId = truck.Id }, default);

        Assert.False(result.IsSuccess);
        Assert.Contains("ContainerTruck", result.Error);
        Assert.Null(load.AssignedTruckId);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownTruck_Fails()
    {
        var (load, _) = Wire(LoadType.GeneralFreight, TruckType.FreightTruck);

        var result = await _sut.Handle(new AssignLoadToTruckCommand { LoadId = load.Id, TruckId = Guid.NewGuid() }, default);

        Assert.False(result.IsSuccess);
        Assert.Contains("Truck not found", result.Error);
    }

    [Fact]
    public async Task Handle_NullTruck_UnassignsWithoutTypeCheck()
    {
        var (load, truck) = Wire(LoadType.Vehicle, TruckType.CarHauler);
        load.AssignedTruckId = truck.Id;

        var result = await _sut.Handle(new AssignLoadToTruckCommand { LoadId = load.Id, TruckId = null }, default);

        Assert.True(result.IsSuccess);
        Assert.Null(load.AssignedTruckId);
    }

    [Fact]
    public async Task Handle_LoadOnTrip_FailsWithoutSaving()
    {
        var (load, truck) = Wire(LoadType.GeneralFreight, TruckType.FreightTruck);
        var trip = Trip.Create("Trip", null, [load]);
        load.TripStops.Add(trip.Stops[0]);

        var result = await _sut.Handle(new AssignLoadToTruckCommand { LoadId = load.Id, TruckId = truck.Id }, default);

        Assert.Contains("Change the trip's truck instead.", result.Error);
        Assert.Null(load.AssignedTruckId);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private (Load Load, Truck Truck) Wire(LoadType loadType, TruckType truckType)
    {
        var load = LoadInvoiceChargesTests.NewLoad();
        load.Type = loadType;
        var truck = new Truck { Number = "T-1", Type = truckType };
        _loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);
        _truckRepo.GetByIdAsync(truck.Id, Arg.Any<CancellationToken>()).Returns(truck);
        return (load, truck);
    }
}
