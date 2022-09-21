namespace Logistics.Domain.Shared.Enums;

public static class LoadStatuses
{
    public static IEnumerable<EnumType> GetValues()
    {
        yield return new EnumType("dispatched", "Dispatched");
        yield return new EnumType("pickedup", "Picked Up");
        yield return new EnumType("delivered", "Delivered");
    }
}