using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class LoadServiceTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IVehicleTransportFeatureChecker _vehicleTransportFeatureChecker = Substitute.For<IVehicleTransportFeatureChecker>();
    private readonly Employee _dispatcher = TestData.Driver();
    private readonly Customer _customer = new() { Name = "Acme" };
    private readonly Truck _truck = TestData.Truck();
    private readonly LoadService _sut;

    public LoadServiceTests()
    {
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _vehicleTransportFeatureChecker.CheckLoadTypesAsync(Arg.Any<IEnumerable<LoadType>?>()).Returns(Result.Ok());
        _tenantUow.Stub(_dispatcher);
        _tenantUow.Stub(_customer);
        _tenantUow.Stub(_truck);
        _tenantUow.Stub<Trip>();
        _sut = new LoadService(_tenantUow, _vehicleTransportFeatureChecker);
    }

    /// <summary>One place copies the load details onto the entity, so a new field can't be dropped silently.</summary>
    [Fact]
    public async Task CreateLoadAsync_FullDetails_CopiesEveryFieldOntoTheLoad()
    {
        var details = TestData.TripLoadCommand() with
        {
            AssignedDispatcherId = _dispatcher.Id,
            CustomerId = _customer.Id,
            Source = LoadSource.Pdf,
            RequestedPickupDate = new DateTime(2026, 10, 20, 8, 0, 0, DateTimeKind.Utc),
            RequestedDeliveryDate = new DateTime(2026, 10, 22, 8, 0, 0, DateTimeKind.Utc),
            PickupAppointmentFrom = new DateTime(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc),
            DeliveryAppointmentTo = new DateTime(2026, 10, 22, 17, 0, 0, DateTimeKind.Utc),
            Notes = "Dock 4",
            ContainerId = Guid.NewGuid(),
            OriginTerminalId = Guid.NewGuid(),
            DestinationTerminalId = Guid.NewGuid(),
            HazmatClass = HazmatClass.Class2,
            UnNumber = "UN1203",
            FuelSurcharge = 120,
            Accessorials = [new LoadAccessorialDto { Name = "Lumper", Rate = 75 }],
            FreightClass = FreightClass.Class70,
            NmfcCode = "123456",
            PalletCount = 10,
            PieceCount = 40,
            Weight = 8000,
            Dimensions = "48x40x60",
            IsStackable = true
        };

        var result = await _sut.CreateLoadAsync(new NewLoad(details, _truck.Id), saveChanges: false);

        var load = result.Value!;
        Assert.Equal(_truck.Id, load.AssignedTruckId);
        Assert.Equal(_customer.Id, load.CustomerId);
        Assert.Equal(_dispatcher.Id, load.AssignedDispatcherId);
        Assert.Equal(300_000, load.Distance);
        Assert.Equal(LoadSource.Pdf, load.Source);
        Assert.Equal(details.RequestedPickupDate, load.RequestedPickupDate);
        Assert.Equal(details.RequestedDeliveryDate, load.RequestedDeliveryDate);
        Assert.Equal(details.PickupAppointmentFrom, load.PickupAppointmentFrom);
        Assert.Equal(details.DeliveryAppointmentTo, load.DeliveryAppointmentTo);
        Assert.Equal("Dock 4", load.Notes);
        Assert.Equal(details.ContainerId, load.ContainerId);
        Assert.Equal(details.OriginTerminalId, load.OriginTerminalId);
        Assert.Equal(details.DestinationTerminalId, load.DestinationTerminalId);
        Assert.Equal(HazmatClass.Class2, load.HazmatClass);
        Assert.Equal("UN1203", load.UnNumber);
        Assert.Equal(120, load.FuelSurcharge);
        Assert.Equal("Lumper", Assert.Single(load.Accessorials).Name);
        Assert.Equal(FreightClass.Class70, load.FreightClass);
        Assert.Equal("123456", load.NmfcCode);
        Assert.Equal(10, load.PalletCount);
        Assert.Equal(40, load.PieceCount);
        Assert.Equal(8000, load.Weight);
        Assert.Equal("48x40x60", load.Dimensions);
        Assert.True(load.IsStackable);
    }
}
