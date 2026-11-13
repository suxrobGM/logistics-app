using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Maps tenant presets to features. Presets are copied into feature configs when applied, so a
/// change here reaches only new tenants and presets an admin re-applies.
/// </summary>
public static class TenantPresetCatalog
{
    private static readonly Dictionary<TenantFeature, TenantPreset> CargoFeatures = new()
    {
        [TenantFeature.VehicleTransport] = TenantPreset.CarHauler,
        [TenantFeature.IntermodalContainers] = TenantPreset.Intermodal
    };

    private static readonly TenantFeature[] SoloExcludedFeatures = [TenantFeature.Payroll, TenantFeature.Timesheets];

    /// <summary>
    /// Every feature is on except cargo features whose preset is missing and, for solo, the team features.
    /// The subscription plan still limits the result.
    /// </summary>
    public static IReadOnlySet<TenantFeature> Resolve(IReadOnlyCollection<TenantPreset> presets)
    {
        var features = Enum.GetValues<TenantFeature>()
            .Where(f => !CargoFeatures.TryGetValue(f, out var preset) || presets.Contains(preset))
            .ToHashSet();

        if (presets.Contains(TenantPreset.SoloOperator))
        {
            features.ExceptWith(SoloExcludedFeatures);
        }

        return features;
    }

    public static bool IsValid(IReadOnlyCollection<TenantPreset> presets) =>
        presets.All(Enum.IsDefined) && presets.Any(p => p != TenantPreset.SoloOperator);
}
