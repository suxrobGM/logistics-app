using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools.Dispatch;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Dispatch;

public class EquipmentFitTests
{
    [Theory]
    [InlineData(LoadType.RefrigeratedGoods, TruckType.Reefer, null, "Match")]
    [InlineData(LoadType.RefrigeratedGoods, TruckType.FreightTruck, TrailerType.Reefer, "Match")]
    [InlineData(LoadType.RefrigeratedGoods, TruckType.FreightTruck, null, "Unconfirmed")]
    [InlineData(LoadType.RefrigeratedGoods, TruckType.FreightTruck, TrailerType.DryVan, "Mismatch")]
    [InlineData(LoadType.Liquid, TruckType.FreightTruck, TrailerType.Tanker, "Match")]
    [InlineData(LoadType.OversizeHeavy, TruckType.FreightTruck, TrailerType.Lowboy, "Match")]
    [InlineData(LoadType.Bulk, TruckType.Flatbed, null, "Mismatch")]
    [InlineData(LoadType.GeneralFreight, TruckType.BoxTruck, null, "Match")]
    [InlineData(LoadType.GeneralFreight, TruckType.FreightTruck, TrailerType.Conestoga, "Match")]
    [InlineData(LoadType.GeneralFreight, TruckType.FreightTruck, null, "Unconfirmed")]
    [InlineData(LoadType.GeneralFreight, TruckType.FreightTruck, TrailerType.Tanker, "Mismatch")]
    public void Evaluate_RanksTheTruckAndHookedTrailer(
        LoadType load, TruckType truck, TrailerType? trailer, string expected)
    {
        Assert.Equal(Enum.Parse<EquipmentMatch>(expected), EquipmentFit.Evaluate(load, truck, trailer).Match);
    }

    [Fact]
    public void Evaluate_Mismatch_SaysWhatTheLoadNeeds()
    {
        var (_, note) = EquipmentFit.Evaluate(LoadType.Liquid, TruckType.Flatbed, null);

        Assert.Equal("needs a Tanker truck or trailer", note);
    }
}
