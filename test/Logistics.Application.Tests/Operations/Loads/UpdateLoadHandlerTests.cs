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
