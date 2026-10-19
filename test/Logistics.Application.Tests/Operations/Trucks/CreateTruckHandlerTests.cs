using System.Linq.Expressions;
using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Modules.Operations.Trucks.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trucks;

public class CreateTruckHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Truck, Guid> _truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<Employee, Guid> _employeeRepo = Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly List<Truck> _trucks = [];
    private readonly Employee _driver = new()
    {
        Email = "driver@test.com",
        FirstName = "Jane",
        LastName = "Doe",
        Salary = Money.Zero("USD")
    };
    private readonly CreateTruckHandler _sut;

    public CreateTruckHandlerTests()
    {
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _tenantUow.Repository<Employee>().Returns(_employeeRepo);
        _truckRepo.GetAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => _trucks.AsQueryable().FirstOrDefault(ci.Arg<Expression<Func<Truck, bool>>>()));
        _employeeRepo.GetByIdAsync(_driver.Id, Arg.Any<CancellationToken>()).Returns(_driver);
        var guard = Substitute.For<IVehicleTransportGuard>();
        guard.CheckTruckTypeAsync(Arg.Any<TruckType?>()).Returns(Result.Ok());
        _sut = new CreateTruckHandler(_tenantUow, guard);
    }

    [Fact]
    public async Task Handle_DriverIsSecondaryOnAnotherTruck_FailsWithoutAdding()
    {
        _trucks.Add(new Truck { Number = "100", Type = TruckType.FreightTruck, SecondaryDriverId = _driver.Id });

        var result = await _sut.Handle(Command("101"), default);

        Assert.Contains("already drives truck '100'", result.Error);
        await _truckRepo.DidNotReceive().AddAsync(Arg.Any<Truck>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NumberTakenInOtherCase_Fails()
    {
        _trucks.Add(new Truck { Number = "TRK-100", Type = TruckType.FreightTruck });

        var result = await _sut.Handle(Command(" trk-100 "), default);

        Assert.False(result.IsSuccess);
        await _truckRepo.DidNotReceive().AddAsync(Arg.Any<Truck>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FreeDriver_AddsTruckWithTrimmedNumberAndCanonicalVin()
    {
        var result = await _sut.Handle(Command(" TRK-101 ", vin: " 1hgbh41jxmn109186 "), default);

        Assert.True(result.IsSuccess);
        await _truckRepo.Received(1).AddAsync(
            Arg.Is<Truck>(t => t.Number == "TRK-101" && t.Vin == "1HGBH41JXMN109186"), Arg.Any<CancellationToken>());
    }

    private CreateTruckCommand Command(string number, string? vin = null) => new()
    {
        TruckNumber = number,
        TruckType = TruckType.FreightTruck,
        MainDriverId = _driver.Id,
        Vin = vin
    };
}
