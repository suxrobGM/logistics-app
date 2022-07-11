namespace Logistics.Domain.Shared;

public static class LoadStatuses
{
    public static IEnumerable<EnumType> GetValues()
    {
        yield return new EnumType("ready", "Ready");
        yield return new EnumType("loaded", "Loaded");
        yield return new EnumType("offduty", "Off duty");
    }
}