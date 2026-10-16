using FluentValidation.TestHelper;
using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class TripNewLoadTests
{
    #region Mapping

    [Fact]
    public void ToCreateLoadParameters_FullLoad_CarriesEveryField()
    {
        var truckId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var load = ValidLoad() with
        {
            Source = LoadSource.Pdf,
            RequestedPickupDate = new DateTime(2026, 10, 20, 8, 0, 0, DateTimeKind.Utc),
            RequestedDeliveryDate = new DateTime(2026, 10, 22, 8, 0, 0, DateTimeKind.Utc),
            Notes = "Dock 4",
            ContainerId = Guid.NewGuid(),
            OriginTerminalId = Guid.NewGuid(),
            DestinationTerminalId = Guid.NewGuid(),
            IsHazmat = true,
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

        var p = load.ToCreateLoadParameters(truckId, tripId);

        Assert.Equal(truckId, p.TruckId);
        Assert.Equal(tripId, p.TripId);
        Assert.Equal(load.CustomerId, p.CustomerId);
        Assert.Equal(load.AssignedDispatcherId, p.DispatcherId);
        Assert.Equal(LoadSource.Pdf, p.Source);
        Assert.Equal(load.RequestedPickupDate, p.RequestedPickupDate);
        Assert.Equal(load.RequestedDeliveryDate, p.RequestedDeliveryDate);
        Assert.Equal("Dock 4", p.Notes);
        Assert.Equal(load.ContainerId, p.ContainerId);
        Assert.Equal(load.OriginTerminalId, p.OriginTerminalId);
        Assert.Equal(load.DestinationTerminalId, p.DestinationTerminalId);
        Assert.True(p.IsHazmat);
        Assert.Equal(HazmatClass.Class2, p.HazmatClass);
        Assert.Equal("UN1203", p.UnNumber);
        Assert.Equal(120, p.FuelSurcharge);
        Assert.Equal("Lumper", Assert.Single(p.Accessorials!).Name);
        Assert.Equal(FreightClass.Class70, p.FreightClass);
        Assert.Equal("123456", p.NmfcCode);
        Assert.Equal(10, p.PalletCount);
        Assert.Equal(40, p.PieceCount);
        Assert.Equal(8000, p.Weight);
        Assert.Equal("48x40x60", p.Dimensions);
        Assert.True(p.IsStackable);
    }

    #endregion

    #region Validation

    [Fact]
    public void CreateTripValidator_ValidNewLoad_Passes()
    {
        var command = new CreateTripCommand { Name = "Trip", NewLoads = [ValidLoad()] };

        var result = new CreateTripValidator().TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTripValidator_NegativeFuelSurcharge_Fails()
    {
        var command = new CreateTripCommand { Name = "Trip", NewLoads = [ValidLoad() with { FuelSurcharge = -1 }] };

        var result = new CreateTripValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor("NewLoads[0].FuelSurcharge");
    }

    [Fact]
    public void CreateTripValidator_InvalidAccessorial_Fails()
    {
        var command = new CreateTripCommand
        {
            Name = "Trip",
            NewLoads = [ValidLoad() with { Accessorials = [new LoadAccessorialDto { Name = "", Rate = 10 }] }]
        };

        var result = new CreateTripValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor("NewLoads[0].Accessorials[0].Name");
    }

    [Fact]
    public void UpdateTripValidator_NegativeWeightOnNewLoad_Fails()
    {
        var command = new UpdateTripCommand { TripId = Guid.NewGuid(), NewLoads = [ValidLoad() with { Weight = -5 }] };

        var result = new UpdateTripValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor("NewLoads[0].Weight");
    }

    #endregion

    private static CreateTripLoadCommand ValidLoad() => new()
    {
        Name = "Freight",
        Type = LoadType.GeneralFreight,
        OriginAddress = new Address { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" },
        OriginLocation = new GeoPoint(-96.8, 32.8),
        DestinationAddress = new Address { Line1 = "2 Main", City = "Austin", State = "TX", ZipCode = "73301", Country = "US" },
        DestinationLocation = new GeoPoint(-97.7, 30.3),
        DeliveryCost = 1500,
        Distance = 300_000,
        AssignedDispatcherId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid()
    };
}
