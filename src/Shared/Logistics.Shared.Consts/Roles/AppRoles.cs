namespace Logistics.Shared.Consts.Roles;

public static class AppRoles
{
    public const string SuperAdmin = "app.superadmin";
    public const string Admin = "app.admin";
    public const string Manager = "app.manager";

    public static IEnumerable<EnumType<string>> GetValues()
    {
        yield return new EnumType<string>(SuperAdmin, "Super Admin");
        yield return new EnumType<string>(Admin, "Admin");
        yield return new EnumType<string>(Manager, "Manager");
    }
}
