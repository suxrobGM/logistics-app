using Logistics.Domain.Primitives.Enums;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class TruckLoadCompatibilityTests
{
    [Theory]
    [InlineData(TruckType.CarHauler, LoadType.Vehicle, true)]
    [InlineData(TruckType.CarTransporter, LoadType.Vehicle, true)]
    [InlineData(TruckType.FreightTruck, LoadType.Vehicle, false)]
    [InlineData(TruckType.CarHauler, LoadType.GeneralFreight, false)]
    [InlineData(TruckType.ContainerTruck, LoadType.IntermodalContainer, true)]
    [InlineData(TruckType.ContainerTruck, LoadType.ReeferContainer, true)]
    [InlineData(TruckType.ContainerTruck, LoadType.TankContainer, true)]
    [InlineData(TruckType.Reefer, LoadType.ReeferContainer, false)]
    [InlineData(TruckType.ContainerTruck, LoadType.Vehicle, false)]
    [InlineData(TruckType.FreightTruck, LoadType.RefrigeratedGoods, true)]
    [InlineData(TruckType.Flatbed, LoadType.HazardousMaterials, true)]
    public void CanHaul_EnforcesOnlyTheSpecializedPairings(TruckType truck, LoadType load, bool expected)
    {
        Assert.Equal(expected, TruckLoadCompatibility.CanHaul(truck, load));
    }

    [Fact]
    public void DescribeMismatch_CompatiblePair_ReturnsNull()
    {
        Assert.Null(TruckLoadCompatibility.DescribeMismatch("T-1", TruckType.CarHauler, LoadType.Vehicle));
    }

    [Fact]
    public void DescribeMismatch_SpecializedTruck_NamesWhatItCarries()
    {
        var message = TruckLoadCompatibility.DescribeMismatch("T-1", TruckType.ContainerTruck, LoadType.GeneralFreight);

        Assert.Equal(
            "Truck T-1 is a ContainerTruck and can only carry IntermodalContainer, TankContainer, ReeferContainer loads.",
            message);
    }

    [Fact]
    public void DescribeMismatch_SpecializedLoad_NamesTheTrucksItNeeds()
    {
        var message = TruckLoadCompatibility.DescribeMismatch("T-1", TruckType.FreightTruck, LoadType.Vehicle);

        Assert.Equal("A Vehicle load needs a CarHauler or CarTransporter; truck T-1 is a FreightTruck.", message);
    }
}
