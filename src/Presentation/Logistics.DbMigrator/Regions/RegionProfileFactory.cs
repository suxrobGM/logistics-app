using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Regions;

/// <summary>
/// Resolves the <see cref="IRegionProfile"/> implementation for a given <see cref="Region"/>.
/// </summary>
public interface IRegionProfileFactory
{
    IRegionProfile Get(Region region);
}

internal sealed class RegionProfileFactory(IEnumerable<IRegionProfile> profiles) : IRegionProfileFactory
{
    private readonly IReadOnlyDictionary<Region, IRegionProfile> profiles = profiles.ToDictionary(p => p.Region);

    public IRegionProfile Get(Region region)
    {
        return profiles.TryGetValue(region, out var profile)
            ? profile
            : throw new InvalidOperationException($"No region profile registered for {region}.");
    }
}
