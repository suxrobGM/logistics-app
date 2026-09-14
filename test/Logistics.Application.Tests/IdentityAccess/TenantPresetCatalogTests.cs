using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess;

public class TenantPresetCatalogTests
{
    [Fact]
    public void Resolve_GeneralFreight_TurnsOffCargoFeatures()
    {
        var features = TenantPresetCatalog.Resolve([TenantPreset.GeneralFreight]);

        Assert.DoesNotContain(TenantFeature.VehicleTransport, features);
        Assert.DoesNotContain(TenantFeature.IntermodalContainers, features);
        Assert.Contains(TenantFeature.Payroll, features);
    }

    [Fact]
    public void Resolve_MixedCargoPresets_CombinesTheirFeatures()
    {
        var features = TenantPresetCatalog.Resolve([TenantPreset.CarHauler, TenantPreset.Intermodal]);

        Assert.Contains(TenantFeature.VehicleTransport, features);
        Assert.Contains(TenantFeature.IntermodalContainers, features);
    }

    [Fact]
    public void Resolve_SoloWithCarHauler_KeepsVehicleTransportAndDropsTeamFeatures()
    {
        var features = TenantPresetCatalog.Resolve([TenantPreset.CarHauler, TenantPreset.SoloOperator]);

        Assert.Contains(TenantFeature.VehicleTransport, features);
        Assert.DoesNotContain(TenantFeature.IntermodalContainers, features);
        Assert.DoesNotContain(TenantFeature.Payroll, features);
        Assert.DoesNotContain(TenantFeature.Timesheets, features);
    }

    [Theory]
    [InlineData(new TenantPreset[0], false)]
    [InlineData(new[] { TenantPreset.SoloOperator }, false)]
    [InlineData(new[] { TenantPreset.GeneralFreight }, true)]
    [InlineData(new[] { TenantPreset.CarHauler, TenantPreset.SoloOperator }, true)]
    public void IsValid_RequiresACargoPreset(TenantPreset[] presets, bool expected)
    {
        Assert.Equal(expected, TenantPresetCatalog.IsValid(presets));
    }
}
