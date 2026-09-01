using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Identity.Roles;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

/// <summary>
/// Drivers hold tenant-wide Load.View, so the scope lives here rather than in the controller:
/// GetLoadTool and CreateLoadInvoiceTool send this same query from the AI surface.
/// </summary>
public class GetLoadByIdHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Load, Guid> loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();

    private readonly Guid driverId = Guid.NewGuid();
    private readonly GetLoadByIdHandler sut;

    public GetLoadByIdHandlerTests()
    {
        tenantUow.Repository<Load>().Returns(loadRepo);
        currentUserService.GetUserId().Returns(driverId);
        sut = new GetLoadByIdHandler(tenantUow, currentUserService);
    }

    private static Load CreateLoad(Truck? assignedTruck) => new()
    {
        Name = "Load 1",
        Type = LoadType.GeneralFreight,
        OriginAddress = new Address { Line1 = "1 A St", City = "NYC", State = "NY", ZipCode = "10001", Country = "US" },
        OriginLocation = new GeoPoint(-74.0, 40.7),
        DestinationAddress = new Address { Line1 = "2 B St", City = "LA", State = "CA", ZipCode = "90001", Country = "US" },
        DestinationLocation = new GeoPoint(-118.2, 34.0),
        DeliveryCost = new Money { Amount = 1000m, Currency = "USD" },
        Customer = new Customer { Name = "ACME" },
        AssignedTruck = assignedTruck,
        AssignedTruckId = assignedTruck?.Id
    };

    private static Truck CreateTruck(Guid? mainDriverId) => new()
    {
        Id = Guid.NewGuid(),
        Number = "101",
        Type = TruckType.FreightTruck,
        MainDriverId = mainDriverId
    };

    [Fact]
    public async Task Handle_DriverAsksForALoadTheyDoNotDrive_ReturnsNotFound()
    {
        currentUserService.IsInRole(TenantRoles.Driver).Returns(true);
        var load = CreateLoad(CreateTruck(Guid.NewGuid()));
        loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);

        var result = await sut.Handle(new GetLoadByIdQuery { Id = load.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_DriverAsksForTheirOwnLoad_ReturnsIt()
    {
        currentUserService.IsInRole(TenantRoles.Driver).Returns(true);
        var load = CreateLoad(CreateTruck(driverId));
        loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);

        var result = await sut.Handle(new GetLoadByIdQuery { Id = load.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_DispatcherAsksForAnyLoad_ReturnsIt()
    {
        currentUserService.IsInRole(TenantRoles.Driver).Returns(false);
        var load = CreateLoad(CreateTruck(Guid.NewGuid()));
        loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);

        var result = await sut.Handle(new GetLoadByIdQuery { Id = load.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UnassignedLoad_IsNotVisibleToADriver()
    {
        currentUserService.IsInRole(TenantRoles.Driver).Returns(true);
        var load = CreateLoad(assignedTruck: null);
        loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);

        var result = await sut.Handle(new GetLoadByIdQuery { Id = load.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
