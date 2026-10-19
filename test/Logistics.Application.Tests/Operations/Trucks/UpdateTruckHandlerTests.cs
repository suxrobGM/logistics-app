using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Modules.Operations.Trailers.Services;
using Logistics.Application.Modules.Operations.Trucks.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trucks;

public class UpdateTruckHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Truck, Guid> _truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<Employee, Guid> _employeeRepo = Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly IVehicleTransportGuard _guard = Substitute.For<IVehicleTransportGuard>();
    private readonly List<Truck> _trucks = [];
    private readonly UpdateTruckHandler _sut;

    public UpdateTruckHandlerTests()
    {
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _tenantUow.Repository<Employee>().Returns(_employeeRepo);
        _truckRepo.Query().Returns(_ => _trucks.BuildMock());
        _truckRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci => _trucks.FirstOrDefault(t => t.Id == ci.Arg<Guid>()));
        _guard.CheckTruckTypeAsync(Arg.Any<TruckType?>()).Returns(Result.Ok());
        _sut = new UpdateTruckHandler(_tenantUow, _guard, Substitute.For<ITrailerHookService>());
    }

    [Theory]
    [InlineData(EmployeeStatus.Suspended)]
    [InlineData(EmployeeStatus.Terminated)]
    [InlineData(EmployeeStatus.OnLeave)]
    public async Task Handle_InactiveMainDriver_FailsWithoutSaving(EmployeeStatus status)
    {
        var truck = AddTruck("101");
        var driver = AddDriver(status);

        var result = await _sut.Handle(new UpdateTruckCommand { Id = truck.Id, MainDriverId = driver.Id }, default);

        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be assigned", result.Error);
        Assert.Null(truck.MainDriverId);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InactiveSecondaryDriver_Fails()
    {
        var truck = AddTruck("101");
        var driver = AddDriver(EmployeeStatus.Terminated);

        var result = await _sut.Handle(new UpdateTruckCommand { Id = truck.Id, SecondaryDriverId = driver.Id }, default);

        Assert.False(result.IsSuccess);
        Assert.Null(truck.SecondaryDriverId);
    }

    [Fact]
    public async Task Handle_ActiveDriver_AssignsAndSaves()
    {
        var truck = AddTruck("101");
        var driver = AddDriver(EmployeeStatus.Active);

        var result = await _sut.Handle(new UpdateTruckCommand { Id = truck.Id, MainDriverId = driver.Id }, default);

        Assert.True(result.IsSuccess);
        Assert.Same(driver, truck.MainDriver);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private Truck AddTruck(string number)
    {
        var truck = new Truck { Number = number, Type = TruckType.FreightTruck };
        _trucks.Add(truck);
        return truck;
    }

    private Employee AddDriver(EmployeeStatus status)
    {
        var driver = new Employee
        {
            Email = $"{Guid.NewGuid():N}@test.com",
            FirstName = "Jane",
            LastName = "Doe",
            Salary = Money.Zero("USD"),
            Status = status
        };
        _employeeRepo.GetByIdAsync(driver.Id, Arg.Any<CancellationToken>()).Returns(driver);
        return driver;
    }
}
