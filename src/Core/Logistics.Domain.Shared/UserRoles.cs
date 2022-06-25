namespace Logistics.Domain.Shared;

public static class UserRoles
{
    public static IEnumerable<EnumType> GetValues()
    {
        yield return new EnumType("guest", "Guest");
        yield return new EnumType("admin", "Admin");
    }
}