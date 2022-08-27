namespace Logistics.Domain.Shared;

public static class AppRoles
{
    public const string Manager = "app.manager";
    public const string Admin = "app.admin";

    public static IEnumerable<EnumType> GetValues()
    {
        yield return new EnumType(Manager, "Manager");
        yield return new EnumType(Admin, "Admin");
    }
}