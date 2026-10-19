using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class UpdateLoadHandlerTests
{
    private const string Vin = "1HGBH41JXMN109186";

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Load, Guid> _loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly IVehicleTransportGuard _vehicleTransportGuard = Substitute.For<IVehicleTransportGuard>();
    private readonly ILoadChargeService _loadChargeService = Substitute.For<ILoadChargeService>();
    private readonly UpdateLoadHandler _sut;

    public UpdateLoadHandlerTests()
    {
        _tenantUow.Repository<Load>().Returns(_loadRepo);
        _vehicleTransportGuard.CheckLoadTypeAsync(Arg.Any<LoadType?>()).Returns(Result.Ok());
        _sut = new UpdateLoadHandler(_tenantUow, _vehicleTransportGuard, _loadChargeService);
    }

    #region Vehicles

    [Fact]
    public async Task Handle_VehiclesOnFreightLoad_Fails()
    {
        var load = Load(LoadType.GeneralFreight);

        var result = await Update(new UpdateLoadCommand { Id = load.Id, Vehicles = [new LoadVehicleInput { Vin = Vin }] });

        Assert.Equal("Only vehicle loads can list vehicles.", result.Error);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TypeChangedAwayFromVehicleWithVehiclesLeft_Fails()
    {
        var load = Load(LoadType.Vehicle);
        load.SetVehicles([new LoadVehicle { Vin = Vin }]);

        var result = await Update(new UpdateLoadCommand { Id = load.Id, Type = LoadType.GeneralFreight });

        Assert.Equal("Only vehicle loads can list vehicles.", result.Error);
    }

    [Fact]
    public async Task Handle_VehiclesOnVehicleLoad_ReplacesThem()
    {
        var load = Load(LoadType.Vehicle);

        var result = await Update(new UpdateLoadCommand { Id = load.Id, Vehicles = [new LoadVehicleInput { Vin = Vin }] });

        Assert.True(result.IsSuccess);
        Assert.Equal(Vin, Assert.Single(load.Vehicles).Vin);
    }

    #endregion

    #region Invoice sync

    [Fact]
    public async Task Handle_NoChargeChange_DoesNotRebuildTheInvoice()
    {
        var load = Load(LoadType.GeneralFreight);

        await Update(new UpdateLoadCommand
        {
            Id = load.Id,
            Notes = "Gate code 1234",
            DeliveryCost = load.DeliveryCost.Amount,
            Status = LoadStatus.Dispatched
        });

        await _loadChargeService.DidNotReceive().SyncInvoiceAsync(Arg.Any<Load>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DeliveryCostChanged_RebuildsTheInvoice()
    {
        var load = Load(LoadType.GeneralFreight);

        await Update(new UpdateLoadCommand { Id = load.Id, DeliveryCost = 1250m });

        await _loadChargeService.Received(1).SyncInvoiceAsync(load, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FuelSurchargeChanged_RebuildsTheInvoice()
    {
        var load = Load(LoadType.GeneralFreight);

        await Update(new UpdateLoadCommand { Id = load.Id, FuelSurcharge = 75m });

        await _loadChargeService.Received(1).SyncInvoiceAsync(load, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CustomerChanged_RebuildsTheInvoice()
    {
        var load = Load(LoadType.GeneralFreight);
        var customer = new Customer { Name = "New Broker" };
        var customerRepo = Substitute.For<ITenantRepository<Customer, Guid>>();
        customerRepo.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _tenantUow.Repository<Customer>().Returns(customerRepo);

        await Update(new UpdateLoadCommand { Id = load.Id, CustomerId = customer.Id });

        Assert.Equal(customer.Id, load.CustomerId);
        await _loadChargeService.Received(1).SyncInvoiceAsync(load, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TypeChanged_RebuildsTheInvoice()
    {
        var load = Load(LoadType.GeneralFreight);

        await Update(new UpdateLoadCommand { Id = load.Id, Type = LoadType.RefrigeratedGoods });

        await _loadChargeService.Received(1).SyncInvoiceAsync(load, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_AccessorialsResent_RebuildsOnlyWhenOneChanged(bool rateChanged)
    {
        var load = Load(LoadType.GeneralFreight);
        var lumper = new LoadAccessorial
        {
            Load = load,
            LoadId = load.Id,
            Name = "Lumper",
            LineItemType = InvoiceLineItemType.Lumper,
            Unit = AccessorialUnit.Flat,
            Rate = 150m
        };
        load.Accessorials.Add(lumper);
        _loadChargeService
            .When(s => s.ReplaceAccessorialsAsync(load, Arg.Any<IReadOnlyList<LoadAccessorialDto>>(),
                Arg.Any<CancellationToken>()))
            .Do(_ => lumper.Rate = rateChanged ? 175m : 150m);

        await Update(new UpdateLoadCommand
        {
            Id = load.Id,
            Accessorials = [new LoadAccessorialDto { Id = lumper.Id, Name = "Lumper", Rate = 150m }]
        });

        await _loadChargeService.Received(rateChanged ? 1 : 0)
            .SyncInvoiceAsync(load, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Truck

    [Fact]
    public async Task Handle_TruckChangeOnLoadOnTrip_Fails()
    {
        var load = Load(LoadType.GeneralFreight);
        var trip = Trip.Create("Trip", null, [load]);
        load.TripStops.Add(trip.Stops[0]);
        var truck = new Truck { Number = "T-2", Type = TruckType.FreightTruck };
        var truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
        truckRepo.GetByIdAsync(truck.Id).Returns(truck);
        _tenantUow.Repository<Truck>().Returns(truckRepo);

        var result = await Update(new UpdateLoadCommand { Id = load.Id, AssignedTruckId = truck.Id });

        Assert.Contains("Change the trip's truck instead.", result.Error);
        Assert.Null(load.AssignedTruckId);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Hazmat

    [Fact]
    public async Task Handle_NoHazmatClass_ClearsTheClassAndUnNumber()
    {
        var load = Load(LoadType.GeneralFreight);
        load.HazmatClass = HazmatClass.Class3;
        load.UnNumber = "UN1203";

        await Update(new UpdateLoadCommand { Id = load.Id, Name = "Renamed" });

        Assert.Null(load.HazmatClass);
        Assert.Null(load.UnNumber);
    }

    #endregion

    #region Route

    [Fact]
    public async Task Handle_RouteChanged_MovesTheTripStopsAndResumsTheTripDistance()
    {
        var load = Load(LoadType.GeneralFreight);
        load.Distance = 1000;
        var trip = Trip.Create("Trip", null, [load], optimizedTotalDistance: 900);
        load.TripStops.Add(trip.Stops[0]);
        load.TripStops.Add(trip.Stops[1]);
        var origin = new Address { Line1 = "9 Elm", City = "Austin", State = "TX", ZipCode = "73301", Country = "US" };
        var destination = new GeoPoint(-97.7, 30.3);

        await Update(new UpdateLoadCommand
        {
            Id = load.Id, OriginAddress = origin, DestinationLocation = destination, Distance = 2500
        });

        var pickup = trip.Stops.Single(s => s.Type == TripStopType.PickUp);
        var dropOff = trip.Stops.Single(s => s.Type == TripStopType.DropOff);
        Assert.Equal(origin, pickup.Address);
        Assert.Equal(destination, dropOff.Location);
        Assert.Equal(2500, trip.TotalDistance);
    }

    #endregion

    private Task<Result> Update(UpdateLoadCommand command) => _sut.Handle(command, CancellationToken.None);

    private Load Load(LoadType type)
    {
        var address = new Address { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" };
        var load = new Load
        {
            Name = "Test Load",
            Type = type,
            Customer = null!,
            OriginAddress = address,
            OriginLocation = new GeoPoint(0, 0),
            DestinationAddress = address,
            DestinationLocation = new GeoPoint(0, 0),
            DeliveryCost = new Money { Amount = 1000m, Currency = "USD" }
        };
        _loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);
        return load;
    }
}
