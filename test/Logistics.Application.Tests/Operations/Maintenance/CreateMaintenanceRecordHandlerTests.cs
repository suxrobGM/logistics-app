using Logistics.Application.Modules.Operations.Maintenance.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Maintenance;

public class CreateMaintenanceRecordHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly Truck _truck = TestData.Truck();
    private readonly CreateMaintenanceRecordHandler _sut;

    public CreateMaintenanceRecordHandlerTests()
    {
        _tenantUow.Stub(_truck);
        _tenantUow.Stub<Employee>();
        _tenantUow.Stub<MaintenanceRecord>();
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
        ServiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
        LaborCost = 100m,
        PartsCost = partsCost,
        Parts = parts
    };

    private static CreateMaintenancePartRequest Part(decimal quantity, decimal unitCost) => new()
    {
        PartNumber = "P-1",
        PartName = "Filter",
        Quantity = quantity,
        UnitCost = unitCost
    };
}
