using FluentValidation.TestHelper;
using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Shared.Models;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class TripNewLoadTests
{
    #region Validation

    [Fact]
    public void CreateTripValidator_ValidNewLoad_Passes()
    {
        var command = new CreateTripCommand { Name = "Trip", NewLoads = [TestData.TripLoadCommand()] };

        var result = new CreateTripValidator().TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTripValidator_NegativeFuelSurcharge_Fails()
    {
        var command = new CreateTripCommand { Name = "Trip", NewLoads = [TestData.TripLoadCommand() with { FuelSurcharge = -1 }] };

        var result = new CreateTripValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor("NewLoads[0].FuelSurcharge");
    }

    [Fact]
    public void CreateTripValidator_InvalidAccessorial_Fails()
    {
        var command = new CreateTripCommand
        {
            Name = "Trip",
            NewLoads = [TestData.TripLoadCommand() with { Accessorials = [new LoadAccessorialDto { Name = "", Rate = 10 }] }]
        };

        var result = new CreateTripValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor("NewLoads[0].Accessorials[0].Name");
    }

    [Fact]
    public void UpdateTripValidator_NegativeWeightOnNewLoad_Fails()
    {
        var command = new UpdateTripCommand { TripId = Guid.NewGuid(), NewLoads = [TestData.TripLoadCommand() with { WeightKg = -5 }] };

        var result = new UpdateTripValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor("NewLoads[0].WeightKg");
    }

    #endregion
}
