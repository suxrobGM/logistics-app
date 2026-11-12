using FluentValidation.TestHelper;
using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class TripNewLoadTests
{
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

    internal static CreateTripLoadCommand ValidLoad() => new()
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
