namespace Logistics.Domain.Shared;

public static class EmployeeRoles
{
    public static IEnumerable<EnumType> GetValues()
    {
        yield return new EnumType("guest", "Guest");
        yield return new EnumType("driver", "Driver");
        yield return new EnumType("dispatcher", "Dispatcher");
        yield return new EnumType("manager", "Manager");
        yield return new EnumType("owner", "Owner");
    }
}