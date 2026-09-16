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

public class GetLoadByIdHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Load, Guid> _loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private readonly Guid _driverId = Guid.NewGuid();
    private readonly GetLoadByIdHandler _sut;

    public GetLoadByIdHandlerTests()
    {
        _tenantUow.Repository<Load>().Returns(_loadRepo);
        _currentUserService.GetUserId().Returns(_driverId);
        _sut = new GetLoadByIdHandler(_tenantUow, _currentUserService);
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
        _currentUserService.IsInRole(TenantRoles.Driver).Returns(true);
        var load = CreateLoad(CreateTruck(Guid.NewGuid()));
        _loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);

        var result = await _sut.Handle(new GetLoadByIdQuery { Id = load.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_DriverAsksForTheirOwnLoad_ReturnsIt()
    {
        _currentUserService.IsInRole(TenantRoles.Driver).Returns(true);
        var load = CreateLoad(CreateTruck(_driverId));
        _loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);

        var result = await _sut.Handle(new GetLoadByIdQuery { Id = load.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_DispatcherAsksForAnyLoad_ReturnsIt()
    {
        _currentUserService.IsInRole(TenantRoles.Driver).Returns(false);
        var load = CreateLoad(CreateTruck(Guid.NewGuid()));
        _loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);

        var result = await _sut.Handle(new GetLoadByIdQuery { Id = load.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_PlatformAdminWithDriverRole_ReturnsAnyLoad()
    {
        _currentUserService.IsInRole(TenantRoles.Driver).Returns(true);
        _currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin).Returns(true);
        var load = CreateLoad(CreateTruck(Guid.NewGuid()));
        _loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);

        var result = await _sut.Handle(new GetLoadByIdQuery { Id = load.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UnassignedLoad_IsNotVisibleToADriver()
    {
        _currentUserService.IsInRole(TenantRoles.Driver).Returns(true);
        var load = CreateLoad(assignedTruck: null);
        _loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);

        var result = await _sut.Handle(new GetLoadByIdQuery { Id = load.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
