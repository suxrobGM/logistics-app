using Logistics.Application.Abstractions.Notifications;
using Logistics.Application.Modules.Operations.Loads.Events;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class LoadAssignedToTruckNotificationHandlerTests
{
    private readonly IPushNotificationService _push = Substitute.For<IPushNotificationService>();
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Load, Guid> _loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly LoadAssignedToTruckNotificationHandler _sut;

    public LoadAssignedToTruckNotificationHandlerTests()
    {
        _tenantUow.Repository<Load>().Returns(_loadRepo);
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _sut = new LoadAssignedToTruckNotificationHandler(
            _push,
            Substitute.For<INotificationService>(),
            Substitute.For<ITelegramNotificationService>(),
            _tenantUow,
            NullLogger<LoadAssignedToTruckNotificationHandler>.Instance);
    }

    /// <summary>The event is raised before the insert assigns the number (a private, database-set property), so it must come off the saved load.</summary>
    [Fact]
    public async Task Handle_UsesNumberFromTrackedLoad()
    {
        var load = LoadInvoiceChargesTests.NewLoad();
        typeof(Load).GetProperty(nameof(Load.Number))!.SetValue(load, 1234L);
        _loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);

        await _sut.Handle(
            new LoadAssignedToTruckEvent(load.Id, Guid.NewGuid(), "T-1", "token", null, "Driver"),
            CancellationToken.None);

        await _push.Received(1).SendNotificationAsync(
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("#1234")),
            "token",
            Arg.Any<IReadOnlyDictionary<string, string>?>());
    }
}
