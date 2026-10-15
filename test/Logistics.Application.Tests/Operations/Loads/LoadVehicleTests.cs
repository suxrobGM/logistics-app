using FluentValidation.TestHelper;
using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Application.Validators;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class LoadVehicleTests
{
    private const string ValidVin = "1HGBH41JXMN109186";

    private readonly LoadVehicleInputValidator _sut = new();

    #region Vehicle validation

    [Fact]
    public void Validate_VinOnly_Passes()
    {
        var result = _sut.TestValidate(new LoadVehicleInput { Vin = ValidVin });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_LowercaseVinWithSpaces_Passes()
    {
        var result = _sut.TestValidate(new LoadVehicleInput { Vin = $" {ValidVin.ToLowerInvariant()} " });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_MakeWithoutVin_Passes()
    {
        var result = _sut.TestValidate(new LoadVehicleInput { Make = "Toyota", Model = "Camry", Year = 2021 });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NeitherVinNorMake_Fails()
    {
        var result = _sut.TestValidate(new LoadVehicleInput { Model = "Camry", LotNumber = "LOT-1" });
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("1HGBH41JXMN10918")] // 16 characters
    [InlineData("1HGBH41JXMN1091867")] // 18 characters
    [InlineData("1HGBH41JXMN1O9186")] // contains O
    [InlineData("1HGBH41JXMN1-9186")]
    public void Validate_MalformedVin_FailsOnVin(string vin)
    {
        var result = _sut.TestValidate(new LoadVehicleInput { Vin = vin });
        result.ShouldHaveValidationErrorFor(v => v.Vin);
    }

    [Fact]
    public void Validate_NegativeRate_Fails()
    {
        var result = _sut.TestValidate(new LoadVehicleInput { Vin = ValidVin, Rate = -1 });
        result.ShouldHaveValidationErrorFor(v => v.Rate);
    }

    [Fact]
    public void CreateLoadValidator_VehiclesOnFreightLoad_Fails()
    {
        var command = ValidCreateCommand(LoadType.GeneralFreight);
        command.Vehicles = [new LoadVehicleInput { Vin = ValidVin }];

        var result = new CreateLoadValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Vehicles);
    }

    [Fact]
    public void CreateLoadValidator_VehicleLoadWithBadVin_FailsOnThatVehicle()
    {
        var command = ValidCreateCommand(LoadType.Vehicle);
        command.Vehicles = [new LoadVehicleInput { Vin = ValidVin }, new LoadVehicleInput { Vin = "SHORT" }];

        var result = new CreateLoadValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor("Vehicles[1].Vin");
        result.ShouldNotHaveValidationErrorFor("Vehicles[0].Vin");
    }

    [Fact]
    public void CreateTripValidator_NewVehicleLoadWithBadVin_Fails()
    {
        var command = new CreateTripCommand
        {
            Name = "Trip",
            NewLoads =
            [
                new CreateTripLoadCommand
                {
                    Name = "Cars",
                    Type = LoadType.Vehicle,
                    OriginAddress = ValidCreateCommand(LoadType.Vehicle).OriginAddress,
                    OriginLocation = new GeoPoint(-96.8, 32.8),
                    DestinationAddress = ValidCreateCommand(LoadType.Vehicle).DestinationAddress,
                    DestinationLocation = new GeoPoint(-97.7, 30.3),
                    AssignedDispatcherId = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    Vehicles = [new LoadVehicleInput { Vin = "SHORT" }]
                }
            ]
        };

        var result = new CreateTripValidator().TestValidate(command);

        Assert.False(result.IsValid);
    }

    #endregion

    #region Input mapping

    [Fact]
    public void ToLoadVehicles_KeepsSentIdAndNormalizesVin()
    {
        var existingId = Guid.NewGuid();

        var vehicles = new[]
        {
            new LoadVehicleInput { Id = existingId, Vin = ValidVin.ToLowerInvariant(), Make = " Ford ", IsOperable = false },
            new LoadVehicleInput { Make = "Toyota", Color = "  " }
        }.ToLoadVehicles();

        Assert.Equal(existingId, vehicles[0].Id);
        Assert.Equal(ValidVin, vehicles[0].Vin);
        Assert.Equal("Ford", vehicles[0].Make);
        Assert.False(vehicles[0].IsOperable);
        Assert.NotEqual(Guid.Empty, vehicles[1].Id);
        Assert.Null(vehicles[1].Color);
    }

    #endregion

    #region Capacity warning

    [Theory]
    [InlineData(7, 7)]
    [InlineData(7, 3)]
    [InlineData(0, 12)] // capacity not set
    public void GetWarning_WithinCapacityOrUnset_ReturnsNull(int capacity, int vehicles)
    {
        Assert.Null(VehicleCapacityChecker.GetWarning("T-1", capacity, vehicles));
    }

    [Fact]
    public void GetWarning_OverCapacity_NamesTruckCapacityAndCount()
    {
        var warning = VehicleCapacityChecker.GetWarning("T-1", 7, 9);

        Assert.NotNull(warning);
        Assert.Contains("T-1", warning);
        Assert.Contains("7", warning);
        Assert.Contains("9", warning);
    }

    #endregion

    private static CreateLoadCommand ValidCreateCommand(LoadType type) => new()
    {
        Name = "Test",
        Type = type,
        AssignedDispatcherId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        OriginAddress = new Address { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" },
        OriginLocation = new GeoPoint(-96.8, 32.8),
        DestinationAddress = new Address { Line1 = "2 Main", City = "Austin", State = "TX", ZipCode = "73301", Country = "US" },
        DestinationLocation = new GeoPoint(-97.7, 30.3),
        Distance = 300_000,
        DeliveryCost = 1500
    };
}
