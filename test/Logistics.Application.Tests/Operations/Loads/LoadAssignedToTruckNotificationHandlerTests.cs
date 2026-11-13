using Logistics.Application.Modules.Operations.Loads.Events;
using Logistics.Application.Modules.Operations.Notifications.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class LoadAssignedToTruckNotificationHandlerTests
{
    private readonly IEventNotifier _notifier = Substitute.For<IEventNotifier>();
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly LoadAssignedToTruckNotificationHandler _sut;

    public LoadAssignedToTruckNotificationHandlerTests()
    {
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _sut = new LoadAssignedToTruckNotificationHandler(_notifier, _tenantUow);
    }

    /// <summary>The event is raised before the insert assigns the number (a private, database-set property), so it must come off the saved load.</summary>
    [Fact]
    public async Task Handle_UsesNumberFromTrackedLoad()
    {
        var load = TestData.Load();
        typeof(Load).GetProperty(nameof(Load.Number))!.SetValue(load, 1234L);
        _tenantUow.Stub(load);

        await _sut.Handle(
            new LoadAssignedToTruckEvent(load.Id, Guid.NewGuid(), "T-1", "token", null, "Driver"),
            CancellationToken.None);

        await _notifier.Received(1).PushAsync(
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("#1234")),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?[]>());
    }
}
