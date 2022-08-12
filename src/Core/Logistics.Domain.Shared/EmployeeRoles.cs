namespace Logistics.Domain.Shared;

public static class EmployeeRoles
{
    public static IEnumerable<EnumType> GetValues()
    {
        yield return new EnumType("guest", "Guest");
        yield return new EnumType("tenant.driver", "Driver");
        yield return new EnumType("tenant.dispatcher", "Dispatcher");
        yield return new EnumType("tenant.manager", "Manager");
        yield return new EnumType("tenant.owner", "Owner");
    }
}