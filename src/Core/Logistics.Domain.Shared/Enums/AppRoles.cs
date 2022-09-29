namespace Logistics.Domain.Shared.Enums;

public static class AppRoles
{
    public const string SuperAdmin = "app.superadmin";
    public const string Admin = "app.admin";
    public const string Manager = "app.manager";

    public static IEnumerable<EnumType> GetValues()
    {
        yield return new EnumType(SuperAdmin, "Super Admin");
        yield return new EnumType(Admin, "Admin");
        yield return new EnumType(Manager, "Manager");
    }
}