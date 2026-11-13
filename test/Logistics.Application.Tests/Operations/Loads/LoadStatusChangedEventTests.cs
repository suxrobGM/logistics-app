using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class LoadStatusChangedEventTests
{
    private static Load NewLoad()
    {
        var load = Load.Create("Load", LoadType.GeneralFreight, 1000, TestData.Address(), new GeoPoint(0, 0), TestData.Address(),
            new GeoPoint(1, 1), new Customer { Name = "Acme" }, TestData.Truck(), TestData.Driver());
        load.DomainEvents.Clear();
        return load;
    }

    private static List<LoadStatusChangedEvent> StatusEvents(Load load) =>
        [.. load.DomainEvents.OfType<LoadStatusChangedEvent>()];

    [Fact]
    public void Dispatch_DraftLoad_RaisesOneEventWithOldAndNewStatus()
    {
        var load = NewLoad();

        load.Dispatch();

        var raised = Assert.Single(StatusEvents(load));
        Assert.Equal(LoadStatus.Draft, raised.OldStatus);
        Assert.Equal(LoadStatus.Dispatched, raised.NewStatus);
        Assert.Equal(load.Id, raised.LoadId);
    }

    [Fact]
    public void UpdateStatus_SameStatus_RaisesNothing()
    {
        var load = NewLoad();

        load.UpdateStatus(LoadStatus.Draft);

        Assert.Empty(StatusEvents(load));
    }

    [Fact]
    public void ConfirmPickupThenDelivery_RaisesAnEventPerStep()
    {
        var load = NewLoad();

        load.Dispatch();
        load.ConfirmPickup();
        load.ConfirmDelivery();

        Assert.Equal(
            [LoadStatus.Dispatched, LoadStatus.PickedUp, LoadStatus.Delivered],
            StatusEvents(load).Select(e => e.NewStatus));
    }

    [Fact]
    public void RestoreImportedStatus_RaisesNothing()
    {
        var load = NewLoad();

        load.RestoreImportedStatus(LoadStatus.Delivered, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        Assert.Empty(StatusEvents(load));
    }
}
