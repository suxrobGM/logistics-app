using Logistics.Application.Modules.Operations.Maintenance.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Maintenance;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Maintenance;

public class CreateMaintenanceRecordHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Truck, Guid> _truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<Employee, Guid> _employeeRepo = Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly ITenantRepository<MaintenanceRecord, Guid> _recordRepo =
        Substitute.For<ITenantRepository<MaintenanceRecord, Guid>>();
    private readonly Truck _truck = new() { Number = "T-1", Type = TruckType.FreightTruck };
    private readonly CreateMaintenanceRecordHandler _sut;

    public CreateMaintenanceRecordHandlerTests()
    {
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _tenantUow.Repository<Employee>().Returns(_employeeRepo);
        _tenantUow.Repository<MaintenanceRecord>().Returns(_recordRepo);
        _truckRepo.GetByIdAsync(_truck.Id, Arg.Any<CancellationToken>()).Returns(_truck);
        _sut = new CreateMaintenanceRecordHandler(_tenantUow);
    }

    public static TheoryData<decimal, decimal> PartsCostCases => new()
    {
        { 0m, 130m },   // itemized parts only
        { 130m, 130m }, // client sent the sum and the items: count it once
        { 999m, 130m }  // items win over a stale lump sum
    };

    [Theory]
    [MemberData(nameof(PartsCostCases))]
    public async Task Handle_WithItemizedParts_PartsCostIsTheirSum(decimal lumpSum, decimal expectedPartsCost)
    {
        var result = await _sut.Handle(Command(lumpSum, [Part(2, 50m), Part(1, 30m)]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPartsCost, result.Value!.PartsCost);
        Assert.Equal(100m + expectedPartsCost, result.Value.TotalCost);
    }

    [Fact]
    public async Task Handle_NoItemizedParts_KeepsTheLumpSum()
    {
        var result = await _sut.Handle(Command(75m, []), CancellationToken.None);

        Assert.Equal(75m, result.Value!.PartsCost);
        Assert.Equal(175m, result.Value.TotalCost);
    }

    [Fact]
    public async Task Handle_UnknownEmployee_FailsWithoutSaving()
    {
        var command = Command(0m, []);
        command.PerformedById = Guid.NewGuid();

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private CreateMaintenanceRecordCommand Command(decimal partsCost, List<CreateMaintenancePartRequest> parts) => new()
    {
        TruckId = _truck.Id,
        Type = Enum.GetValues<MaintenanceType>()[0],
        Description = "Oil change",
        ServiceDate = DateTime.UtcNow,
        LaborCost = 100m,
        PartsCost = partsCost,
        Parts = parts
    };

    private static CreateMaintenancePartRequest Part(int quantity, decimal unitCost) => new()
    {
        PartNumber = "P-1",
        PartName = "Filter",
        Quantity = quantity,
        UnitCost = unitCost
    };
}
